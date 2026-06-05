using TodoApp.Repositories;
using TodoApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register Todo services
builder.Services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/api/health", (IConfiguration configuration, IHostEnvironment environment) =>
{
    if (!string.IsNullOrWhiteSpace(configuration["IS_BROKEN"]))
    {
        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Simulated health failure",
            detail: "The IS_BROKEN setting is enabled, so the demo health endpoint is intentionally returning an error.");
    }

    return Results.Ok(new
    {
        status = "ok",
        environment = environment.EnvironmentName,
        timestamp = DateTimeOffset.UtcNow
    });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Todo}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program;
