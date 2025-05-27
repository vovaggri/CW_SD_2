using FileStoringService.DBFiles;

namespace FileStoringService.Repositories;

public interface IFileMetaRepository
{
    Task<FileMeta?> GetByIdAsync(Guid id);
    Task AddAsync(FileMeta meta);
}