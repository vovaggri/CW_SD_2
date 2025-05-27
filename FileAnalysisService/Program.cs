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

builder.Services
    .AddControllers(options =>
    {
        options.ReturnHttpNotAcceptable = false;
        options.RespectBrowserAcceptHeader = true;
    })
    .AddJsonOptions(opts => {  });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();
    db.Database.Migrate();
}

// Middleware обработки исключений
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwagger(); 
app.UseSwaggerUI();

app.MapControllers();

app.Run();