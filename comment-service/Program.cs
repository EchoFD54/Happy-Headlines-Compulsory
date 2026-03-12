

using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddDbContext<CommentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CommentDb")));

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
    client.BaseAddress = new Uri("http://localhost:5096");
})
.AddPolicyHandler(GetCircuitBreakerPolicy());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.UseHttpsRedirection();

app.MapControllers();

app.Run();
