

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddDbContext<ProfanityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProfanityDb")));

builder.Services.AddScoped<ProfanityService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}




app.MapControllers();

app.Run();
