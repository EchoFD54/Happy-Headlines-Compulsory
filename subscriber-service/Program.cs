

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddDbContext<SubscriberDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SubscriberDb")));

builder.Services.AddScoped<SubscriberService>();
builder.Services.AddScoped<SubscriberQueuePublisher>();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

ChangeToken.OnChange(
    () => app.Configuration.GetReloadToken(),
    () => {
        var isEnabled = app.Configuration.GetValue<bool>("FeatureManagement:SubscriberServiceEnabled");
        var status = isEnabled ? "ENABLED" : "DISABLED";
        
        app.Logger.LogWarning("--------------------------------------------------");
        app.Logger.LogWarning($"[RELEASE TOGGLE] Subscriber Service is now {status}");
        app.Logger.LogWarning("--------------------------------------------------");
    });



app.MapControllers();

app.Run();
