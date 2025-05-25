using FileAnalysisService.Models;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Data;

public class AnalysisDbContext : DbContext
{
    public DbSet<AnalysisResult> AnalysisResults { get; set; }

    public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options)
    {
        
    }
}