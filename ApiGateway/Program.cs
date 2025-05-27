using Api_Gateway.Extension;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ocelot.json",       optional: false, reloadOnChange: true);

builder.Services
    .AddCorsPolicy()    
    .AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandling();
app.UseCors("DefaultCors");
app.MapGet("/health", () => Results.Ok(new { status = "Gateway is up" }));
await app.UseOcelot();

app.Run();