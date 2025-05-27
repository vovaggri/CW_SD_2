using FileStoringService.DBFiles;
using FileStoringService.ExceptionHandler;
using FileStoringService.Repositories;
using FileStoringService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<FilesDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration["ConnectionStrings:Postgres"]))
    .AddScoped<IFileMetaRepository, FileMetaRepository>()
    .AddScoped<IFileStoringService, FileStoringService.Services.FileStoringService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FilesDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseSwagger(); 
app.UseSwaggerUI();
app.MapControllers();
app.Run();