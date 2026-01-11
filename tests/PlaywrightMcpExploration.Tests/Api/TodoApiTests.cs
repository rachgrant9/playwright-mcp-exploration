using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlaywrightMcpExploration.Web.Data;
using PlaywrightMcpExploration.Web.Models;

namespace PlaywrightMcpExploration.Tests.Api;

public class TodoApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _dbName;

    public TodoApiTests(WebApplicationFactory<Program> factory)
    {
        _dbName = $"TestDb_{Guid.NewGuid()}";
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TodoDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing with consistent name
                services.AddDbContext<TodoDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllTodos_WhenNoTodos_ReturnsEmptyList()
    {
        // Arrange - database is already empty

        // Act
        var response = await _client.GetAsync("/api/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<Todo>>();
        todos.Should().NotBeNull();
        todos.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTodos_WhenTodosExist_ReturnsAllTodos()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITodoRepository>();
        
        var todo1 = await repository.CreateAsync(new Todo 
        { 
            Title = "Test Todo 1", 
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        });
        
        var todo2 = await repository.CreateAsync(new Todo 
        { 
            Title = "Test Todo 2", 
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow
        });

        // Act
        var response = await _client.GetAsync("/api/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<Todo>>();
        todos.Should().NotBeNull();
        todos.Should().HaveCount(2);
        todos.Should().ContainSingle(t => t.Title == "Test Todo 1");
        todos.Should().ContainSingle(t => t.Title == "Test Todo 2");
    }

    [Fact]
    public async Task GetTodoById_WhenTodoExists_ReturnsTodo()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITodoRepository>();
        
        var createdTodo = await repository.CreateAsync(new Todo 
        { 
            Title = "Test Todo", 
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        });

        // Act
        var response = await _client.GetAsync($"/api/todos/{createdTodo.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todo = await response.Content.ReadFromJsonAsync<Todo>();
        todo.Should().NotBeNull();
        todo!.Id.Should().Be(createdTodo.Id);
        todo.Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task GetTodoById_WhenTodoDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var response = await _client.GetAsync($"/api/todos/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTodo_WithValidData_ReturnsCreatedWithLocationHeader()
    {
        // Arrange
        var newTodo = new
        {
            Title = "New Todo",
            IsCompleted = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", newTodo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var createdTodo = await response.Content.ReadFromJsonAsync<Todo>();
        createdTodo.Should().NotBeNull();
        createdTodo!.Id.Should().BeGreaterThan(0);
        createdTodo.Title.Should().Be("New Todo");
        createdTodo.IsCompleted.Should().BeFalse();
        createdTodo.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateTodo_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var invalidTodo = new
        {
            Title = "",
            IsCompleted = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", invalidTodo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTodo_WithTitleTooLong_ReturnsBadRequest()
    {
        // Arrange
        var invalidTodo = new
        {
            Title = new string('A', 201), // Exceeds 200 character limit
            IsCompleted = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", invalidTodo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTodo_WithoutTitle_ReturnsBadRequest()
    {
        // Arrange
        var invalidTodo = new
        {
            IsCompleted = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", invalidTodo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTodo_WhenTodoExists_ReturnsOkWithUpdatedTodo()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITodoRepository>();
        
        var createdTodo = await repository.CreateAsync(new Todo 
        { 
            Title = "Original Title", 
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        });

        var updateRequest = new
        {
            Title = "Updated Title",
            IsCompleted = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todos/{createdTodo.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTodo = await response.Content.ReadFromJsonAsync<Todo>();
        updatedTodo.Should().NotBeNull();
        updatedTodo!.Id.Should().Be(createdTodo.Id);
        updatedTodo.Title.Should().Be("Updated Title");
        updatedTodo.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTodo_WhenTodoDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 9999;
        var updateRequest = new
        {
            Title = "Updated Title",
            IsCompleted = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todos/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTodo_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITodoRepository>();
        
        var createdTodo = await repository.CreateAsync(new Todo 
        { 
            Title = "Original Title", 
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        });

        var invalidRequest = new
        {
            Title = "", // Empty title is invalid
            IsCompleted = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todos/{createdTodo.Id}", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteTodo_WhenTodoExists_ReturnsNoContent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITodoRepository>();
        
        var createdTodo = await repository.CreateAsync(new Todo 
        { 
            Title = "Todo to Delete", 
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        });

        // Act
        var response = await _client.DeleteAsync($"/api/todos/{createdTodo.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify todo is actually deleted
        var getResponse = await _client.GetAsync($"/api/todos/{createdTodo.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTodo_WhenTodoDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var response = await _client.DeleteAsync($"/api/todos/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
