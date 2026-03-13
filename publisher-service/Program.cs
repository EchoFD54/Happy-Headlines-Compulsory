using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddScoped<PublisherService>();
builder.Services.AddSingleton<ArticleQueuePublisher>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("publisher-service"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("HappyHeadlines.PublisherService")
            .AddZipkinExporter(options =>
            {
                options.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
            });
    });


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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}




app.MapControllers();

app.Run();
