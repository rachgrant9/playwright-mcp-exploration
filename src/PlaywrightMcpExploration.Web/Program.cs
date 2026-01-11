using Microsoft.EntityFrameworkCore;
using PlaywrightMcpExploration.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with SQLite in-memory database
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("DataSource=:memory:"));

// Register repository
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

var app = builder.Build();

// Ensure database is created on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    dbContext.Database.OpenConnection();
    dbContext.Database.EnsureCreated();
}

app.MapGet("/", () => "Hello World!");

app.Run();
