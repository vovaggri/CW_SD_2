using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FileAnalysisService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileAnalysisService.Controllers;

[ApiController]
[Route("analysis")]
public class AnalysisController : ControllerBase
{
    private readonly IFileAnalysisService _svc;
    private readonly IHttpClientFactory _http;

    public AnalysisController(IFileAnalysisService svc, IHttpClientFactory httpFactory)
    {
        _svc = svc;
        _http = httpFactory;
    }

    /// <summary>
    /// Запустить или получить готовый анализ
    /// </summary>
    [HttpPost("{fileId:guid}")]
    public async Task<IActionResult> Start(Guid fileId)
    {
        var result = await _svc.AnalyzeAsync(fileId);
        return Ok(result);
    }

    /// <summary>
    /// Только получить уже существующий анализ
    /// </summary>
    [HttpGet("{fileId:guid}")]
    public async Task<IActionResult> Get(Guid fileId)
    {
        var result = await _svc.GetResultAsync(fileId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Получить картинку облака слов для данного файла
    /// </summary>
    [HttpGet("{fileId:guid}/cloud")]
    [Produces("image/png")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetWordCloud(Guid fileId)
    {
        // 1. Получаем текст
        var bytes = await _svc.GetFileBytesAsync(fileId);
        var text = Encoding.UTF8.GetString(bytes);

        if (string.IsNullOrWhiteSpace(text))
            return BadRequest(new { error = "Файл пуст или не удалось прочитать текст." });

        var payload = new
        {
            format = "png",
            text = text
        };
        var json = JsonSerializer.Serialize(payload);
        
        var client = _http.CreateClient();
        using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("https://quickchart.io/wordcloud", httpContent);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return StatusCode(502, new { error = $"QuickChart error ({(int)response.StatusCode}): {err}" });
        }

        var imageBytes = await response.Content.ReadAsByteArrayAsync();
        if (imageBytes == null || imageBytes.Length == 0)
            return StatusCode(502, new { error = "QuickChart вернул пустой ответ." });

        return File(imageBytes, "image/png");
    }
}