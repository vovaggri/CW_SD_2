using FileStoringService.DBFiles;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Repositories;

public class FileMetaRepository : IFileMetaRepository
{
    private readonly FilesDbContext _db;
    public FileMetaRepository(FilesDbContext db) => _db = db;
    
    public Task<FileMeta> GetByHashAsync(string hash) => _db.Files.FirstOrDefaultAsync(f => f.Hash == hash);

    public Task<FileMeta> GetByIdAsync(Guid id) => _db.Files.FindAsync(id).AsTask();

    public async Task AddAsync(FileMeta meta)
    {
        _db.Files.Add(meta);
        await _db.SaveChangesAsync();
    }
}