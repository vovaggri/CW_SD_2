using System.Security.Cryptography;
using FileStoringService.DBFiles;
using FileStoringService.Repositories;

namespace FileStoringService.Services;

public class FileStoringService : IFileStoringService
{
    private readonly IFileMetaRepository _repository;
    private readonly IWebHostEnvironment _environment;

    public FileStoringService(IFileMetaRepository repository, IWebHostEnvironment environment)
    {
        _repository = repository;
        _environment = environment;
    }
    
    public async Task<Guid> SaveAsync(IFormFile file)
    {
        var id = Guid.NewGuid();
        var folder= Path.Combine(_environment.ContentRootPath, "Uploaded");
        Directory.CreateDirectory(folder);
        var ext= Path.GetExtension(file.FileName);
        var path = Path.Combine(folder, id + ext);
        
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        await File.WriteAllBytesAsync(path, ms.ToArray());

        var meta = new FileMeta {
            Id = id,
            FileName = file.FileName,
            Location = path,
            UploadedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(meta);
        return id;
    }

    public async Task<(byte[] Content, string FileName)> GetAsync(Guid id)
    {
        var meta = await _repository.GetByIdAsync(id)
                   ?? throw new FileNotFoundException();
        return (await File.ReadAllBytesAsync(meta.Location),
            meta.FileName);
    }
}