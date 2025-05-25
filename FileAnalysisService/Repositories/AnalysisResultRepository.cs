using FileAnalysisService.Data;
using FileAnalysisService.Models;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Repositories;

public class AnalysisResultRepository : IAnalysisResultRepository
{
    private readonly AnalysisDbContext _db;
    public AnalysisResultRepository(AnalysisDbContext db) => _db = db;
    
    public Task<AnalysisResult> GetByFileIdAsync(Guid fileId) => 
        _db.AnalysisResults.FirstOrDefaultAsync(r => r.FileId == fileId);
    
    public Task<AnalysisResult> GetByFileHashAsync(string hash) =>
        _db.AnalysisResults
            .FirstOrDefaultAsync(r => r.FileHash == hash);

    public async Task AddAsync(AnalysisResult result)
    {
        _db.AnalysisResults.Add(result);
        await _db.SaveChangesAsync();
    }
}