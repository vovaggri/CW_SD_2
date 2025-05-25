namespace FileStoringService.Services;

public interface IFileStoringService
{
    Task<Guid> SaveAsync(IFormFile file);
    Task<(byte[] Content, string FileName)> GetAsync(Guid id);
}