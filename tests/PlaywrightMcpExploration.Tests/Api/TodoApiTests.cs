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
}
