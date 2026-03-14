using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddScoped<ArticleService>();
builder.Services.AddSingleton<IArticleDbContextRouter, ArticleDbContextRouter>();
builder.Services.AddHostedService<ArticleQueueConsumer>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("article-service"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("HappyHeadlines.ArticleService") 
            .AddZipkinExporter(options =>
            {
                options.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
            });
    });

var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "redis:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddHostedService<ArticleCachePreloader>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var migrationRunner = new DatabaseMigrationRunner(builder.Configuration);
migrationRunner.MigrateAllDatabases();

var serviceId = Environment.GetEnvironmentVariable("SERVICE_ID") ?? Guid.NewGuid().ToString().Substring(0, 8);

app.Use(async (context, next) =>
{
    Console.WriteLine($"[{serviceId}] Handling request {context.Request.Method} {context.Request.Path}");
    await next();
});


app.UseHttpsRedirection();

app.MapControllers();

app.Run();
