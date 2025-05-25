namespace FileStoringService.DBFiles;
using Microsoft.EntityFrameworkCore;

public class FilesDbContext : DbContext
{
    public DbSet<FileMeta> Files { get; set; }
    public FilesDbContext(DbContextOptions opts) : base(opts) { }
}