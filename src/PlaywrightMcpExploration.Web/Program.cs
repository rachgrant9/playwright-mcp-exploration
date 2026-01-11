using Microsoft.EntityFrameworkCore;
using PlaywrightMcpExploration.Web.Data;
using PlaywrightMcpExploration.Web.Models;
using System.ComponentModel.DataAnnotations;

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

// POST /api/todos - Create a new todo
app.MapPost("/api/todos", async (CreateTodoRequest request, ITodoRepository repository) =>
{
    // Validate request
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(request);
    
    if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
    {
        return Results.BadRequest(validationResults);
    }

    // Create todo
    var todo = new Todo
    {
        Title = request.Title,
        IsCompleted = request.IsCompleted,
        CreatedAt = DateTime.UtcNow
    };

    var createdTodo = await repository.CreateAsync(todo);
    
    return Results.Created($"/api/todos/{createdTodo.Id}", createdTodo);
});

app.Run();

// Make Program class accessible for testing
public partial class Program { }

// Request DTO for creating a todo
public record CreateTodoRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public required string Title { get; init; }
    
    public bool IsCompleted { get; init; }
}
