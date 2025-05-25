namespace FileStoringService.DBFiles;

public class FileMeta
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string Hash { get; set; }
    public string Location { get; set; }
    public DateTime UploadedAt { get; set; }
}