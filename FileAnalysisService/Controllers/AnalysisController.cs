using FileAnalysisService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileAnalysisService.Controllers;

[ApiController]
[Route("analysis")]
public class AnalysisController : ControllerBase
{
    private readonly IFileAnalysisService _svc;
    public AnalysisController(IFileAnalysisService svc)
        => _svc = svc;

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
}