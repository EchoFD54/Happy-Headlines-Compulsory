

using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddDbContext<CommentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CommentDb")));

var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "redis:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));



builder.Services.AddScoped<CommentService>();


static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(){
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30)
        );
}



builder.Services.AddHttpClient<ProfanityServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://profanity-service");
})
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddOpenTelemetry()
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .AddMeter("HappyHeadlines.CommentService") 
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter(); 
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope()){
    var db = scope.ServiceProvider.GetRequiredService<CommentDbContext>();
    db.Database.Migrate(); 
}

app.UseOpenTelemetryPrometheusScrapingEndpoint();

if (app.Environment.IsDevelopment()){
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
