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

// Global exception handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Unhandled exception occurred");
            
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "An unexpected error occurred. Please try again later.",
                details = app.Environment.IsDevelopment() ? error.Error.Message : null
            });
        }
    });
});

// Enable static files and default files
app.UseDefaultFiles();
app.UseStaticFiles();

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
    return todo is null ? Results.NotFound(new { error = $"Todo with id {id} not found" }) : Results.Ok(todo);
});

// POST /api/todos - Create a new todo
app.MapPost("/api/todos", async (CreateTodoRequest request, ITodoRepository repository) =>
{
    // Validate request
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(request);
    
    if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
    {
        var errors = validationResults.Select(v => v.ErrorMessage).ToList();
        return Results.BadRequest(new { error = "Validation failed", errors });
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

// PUT /api/todos/{id} - Update a todo
app.MapPut("/api/todos/{id:int}", async (int id, UpdateTodoRequest request, ITodoRepository repository) =>
{
    // Validate request
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(request);
    
    if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
    {
        var errors = validationResults.Select(v => v.ErrorMessage).ToList();
        return Results.BadRequest(new { error = "Validation failed", errors });
    }

    // Get existing todo first
    var existingTodo = await repository.GetByIdAsync(id);
    if (existingTodo is null)
    {
        return Results.NotFound(new { error = $"Todo with id {id} not found" });
    }

    // Update todo
    var todo = new Todo
    {
        Title = request.Title,
        IsCompleted = request.IsCompleted,
        CreatedAt = existingTodo.CreatedAt
    };

    var updatedTodo = await repository.UpdateAsync(id, todo);
    
    return Results.Ok(updatedTodo);
});

// DELETE /api/todos/{id} - Delete a todo
app.MapDelete("/api/todos/{id:int}", async (int id, ITodoRepository repository) =>
{
    var deleted = await repository.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound(new { error = $"Todo with id {id} not found" });
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

// Request DTO for updating a todo
public record UpdateTodoRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public required string Title { get; init; }
    
    public bool IsCompleted { get; init; }
}
