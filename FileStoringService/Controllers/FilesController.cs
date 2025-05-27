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
    public async Task<IActionResult> UploadFile(
        [FromForm(Name = "file")] IFormFile file)   // ← добавили [FromForm]
    {
        if (file == null)
            return BadRequest(new { error = "No file was provided in the form-data under key 'file'" });

        var id = await _svc.SaveAsync(file);
        return Ok(new { id });  // оборачиваем в объект, чтобы в ответе был {"id":"..."}
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var (buf, name) = await _svc.GetAsync(id);
        return File(buf, "application/octet-stream", name);
    }
}
