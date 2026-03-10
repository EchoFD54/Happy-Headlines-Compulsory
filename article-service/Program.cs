

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddSingleton<ArticleService>();
builder.Services.AddSingleton<IArticleDbContextRouter, ArticleDbContextRouter>();

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
