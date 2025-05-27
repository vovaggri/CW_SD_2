using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FileAnalysisService.Models;
using FileAnalysisService.Repositories;

namespace FileAnalysisService.Services;

public class FileAnalysisService : IFileAnalysisService
{
    private readonly IAnalysisResultRepository _repository;
    private readonly HttpClient _fileClient;

    public FileAnalysisService(IAnalysisResultRepository repository, IHttpClientFactory factory)
    {
        _repository = repository;
        _fileClient = factory.CreateClient("FileStorage");
    }

    public async Task<AnalysisResult> AnalyzeAsync(Guid fileId)
    {
        // Скачиваем байты
        var bytes = await _fileClient.GetByteArrayAsync($"/files/{fileId}");

        // Считаем SHA256
        var hash = ComputeHash(bytes);

        // Проверяем на дубликат
        var existing = await _repository.GetByFileHashAsync(hash);
        if (existing != null)
        {
            var dupResult = new AnalysisResult
            {
                Id                 = Guid.NewGuid(), 
                FileId             = fileId, 
                FileHash           = hash,
                Paragraphs         = existing.Paragraphs,
                Words              = existing.Words,
                Characters         = existing.Characters,
                SimilarityScore    = 1.0,
                CloudImageLocation = existing.CloudImageLocation,
                CreatedAt          = DateTime.UtcNow
            };
            await _repository.AddAsync(dupResult);
            return dupResult;
        }

        // Прежняя статистика
        var text  = Encoding.UTF8.GetString(bytes);
        var stats = ComputeStatistics(text);

        var cloudUrl = BuildWordCloudUrl(text);

        // Генерим URL облака и сохраняем новый результат
        var result = new AnalysisResult {
            Id = Guid.NewGuid(),
            FileId  = fileId,
            FileHash = hash,
            Paragraphs = stats.Paragraphs,
            Words = stats.Words,
            Characters = stats.Characters,
            SimilarityScore = 0.0,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(result);
        return result;
    }

    public Task<AnalysisResult> GetResultAsync(Guid fileId) => _repository.GetByFileIdAsync(fileId);
    
    public Task<byte[]> GetFileBytesAsync(Guid fileId)
        => _fileClient.GetByteArrayAsync($"/files/{fileId}");
    
    private static string ComputeHash(byte[] data)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(data));
    }
    
    private static TextStatistics ComputeStatistics(string text)
    {
        // Параграфы — блоки, разделённые пустой строкой
        var paras = text
            .Split(new[] { "\r\n\r\n", "\n\n" },
                StringSplitOptions.RemoveEmptyEntries)
            .Length;

        // Слова — границы по \w+
        var words = Regex.Matches(text, @"\b\w+\b").Count;

        // Символы — длина строки
        var chars = text.Length;

        return new TextStatistics
        {
            Paragraphs = paras,
            Words      = words,
            Characters = chars
        };
    }
    
    // helper: строим URL к QuickChart Word-Cloud API
    private static string BuildWordCloudUrl(string text)
    {
        // QuickChart Word Cloud принимает текст GET-параметром
        var encoded = Uri.EscapeDataString(text);
        return $"https://quickchart.io/wordcloud?format=png&text={encoded}";
    }
}