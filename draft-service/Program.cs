

using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithThreadName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Service", "DraftService")
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddDbContext<DraftDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DraftDb")));

builder.Services.AddScoped<DraftService>();

builder.Services.AddControllers();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracer =>
    {
        tracer
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.UseHttpsRedirection();

app.MapControllers();

app.Run();
