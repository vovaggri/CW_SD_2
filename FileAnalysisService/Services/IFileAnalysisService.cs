using FileAnalysisService.Models;

namespace FileAnalysisService.Services;

public interface IFileAnalysisService
{
    Task<AnalysisResult> AnalyzeAsync(Guid fileId);
    Task<AnalysisResult> GetResultAsync(Guid fileId);
}