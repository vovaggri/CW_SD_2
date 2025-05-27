using FileAnalysisService.Models;

namespace FileAnalysisService.Repositories;

public interface IAnalysisResultRepository
{
    Task<AnalysisResult?> GetByFileIdAsync(Guid fileId);
    Task<AnalysisResult?> GetByFileHashAsync(string hash);
    Task AddAsync(AnalysisResult result);
}