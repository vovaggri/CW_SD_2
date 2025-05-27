namespace FileAnalysisService.Models;

public class AnalysisResult
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public string FileHash { get; set; }
    public int Paragraphs { get; set; }
    public int Words { get; set; }
    public int Characters { get; set; }
    public double SimilarityScore { get; set; }
    public string? CloudImageLocation { get; set; } 
    public DateTime CreatedAt { get; set; }
}