using Microsoft.EntityFrameworkCore;
using PlaywrightMcpExploration.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with in-memory database
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseInMemoryDatabase("TodoDb"));

// Register repository
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

// GET /api/todos - Get all todos
app.MapGet("/api/todos", async (ITodoRepository repository) =>
{
    var todos = await repository.GetAllAsync();
    return Results.Ok(todos);
});

// GET /api/todos/{id} - Get a specific todo
app.MapGet("/api/todos/{id:int}", async (int id, ITodoRepository repository) =>
{
    var todo = await repository.GetByIdAsync(id);
    return todo is null ? Results.NotFound() : Results.Ok(todo);
});

app.Run();

// Make Program class accessible for testing
public partial class Program { }
