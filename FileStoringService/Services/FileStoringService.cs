// File: Services/FileStoringService.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using FileStoringService.DBFiles;
using FileStoringService.Repositories;

namespace FileStoringService.Services
{
    public class FileStoringService : IFileStoringService
    {
        private readonly IFileMetaRepository _repository;
        private readonly IWebHostEnvironment  _environment;

        public FileStoringService(
            IFileMetaRepository repository,
            IWebHostEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }

        public async Task<Guid> SaveAsync(IFormFile file)
        {
            if (file == null)
                throw new ArgumentException("File parameter is null");

            // 1) генерируем новый ID
            var fileId = Guid.NewGuid();

            // 2) создаём папку, если нужно
            var folder = Path.Combine(_environment.ContentRootPath, "StoredFiles");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // 3) сохраняем бинарник
            var extension = Path.GetExtension(file.FileName);
            var fileName = fileId + extension;
            var fullPath = Path.Combine(folder, fileName);

            using (var fs = new FileStream(fullPath, FileMode.CreateNew))
            {
                await file.CopyToAsync(fs);
            }

            // 4) сохраняем мета-данные
            await _repository.AddAsync(new FileMeta {
                Id = fileId,
                FileName = file.FileName,
                Location = fullPath,
                UploadedAt = DateTime.UtcNow
            });

            return fileId;
        }

        public async Task<(byte[] Content, string FileName)> GetAsync(Guid id)
        {
            var meta = await _repository.GetByIdAsync(id)
                      ?? throw new FileNotFoundException($"No file with id {id}");
            var buf = await File.ReadAllBytesAsync(meta.Location);
            return (buf, meta.FileName);
        }
    }
}
