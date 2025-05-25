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
        // 1) Скачиваем байты
        var bytes = await _fileClient.GetByteArrayAsync($"/files/{fileId}");

        // 2) Считаем SHA256
        var hash = ComputeHash(bytes);

        // 3) Смотрим, не анализировали ли мы уже точно такой файл
        if (await _repository.GetByFileHashAsync(hash) is { } dup)
        {
            // помечаем 100% совпадение
            dup.SimilarityScore = 1.0;
            return dup;
        }

        // 4) Прежняя статистика
        var text  = Encoding.UTF8.GetString(bytes);
        var stats = ComputeStatistics(text);

        var cloudUrl = BuildWordCloudUrl(text);

        // 5) Генерим URL облака и сохраняем новый результат
        var result = new AnalysisResult {
            Id = Guid.NewGuid(),
            FileId  = fileId,
            FileHash = hash,
            Paragraphs = stats.Paragraphs,
            Words = stats.Words,
            Characters = stats.Characters,
            SimilarityScore = 0.0,  // или рассчитанный коэффициент
            CloudImageLocation = cloudUrl,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(result);
        return result;
    }

    public Task<AnalysisResult> GetResultAsync(Guid fileId) => _repository.GetByFileIdAsync(fileId);
    
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