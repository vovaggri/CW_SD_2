using FileAnalysisService.Data;
using FileAnalysisService.Repositories;
using FileAnalysisService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core + PostgreSQL
builder.Services.AddDbContext<AnalysisDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// репозиторий и сервис
builder.Services.AddScoped<IAnalysisResultRepository, AnalysisResultRepository>();
builder.Services.AddScoped<IFileAnalysisService, FileAnalysisService.Services.FileAnalysisService>();

// HttpClient для обращения к FileStoringService
builder.Services.AddHttpClient("FileStorage", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["FileStoring:BaseUrl"]);
});

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware обработки исключений (тот же, что и в остальном проекте)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwagger(); 
app.UseSwaggerUI();

app.MapControllers();

app.Run();