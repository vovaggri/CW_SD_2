using FileStoringService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileStoringService.Controllers;

[ApiController]
[Route("files")]
public class FilesController : ControllerBase
{
    private readonly IFileStoringService _svc;
    public FilesController(IFileStoringService svc) => _svc = svc;

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var id = await _svc.SaveAsync(file);
        return Ok(id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var (buf, name) = await _svc.GetAsync(id);
        return File(buf, "application/octet-stream", name);
    }
}