using Microsoft.EntityFrameworkCore;
using PlaywrightMcpExploration.Web.Data;
using PlaywrightMcpExploration.Web.Models;

namespace PlaywrightMcpExploration.Tests.Data;

public class TodoRepositoryTests : IDisposable
{
    private readonly TodoDbContext _context;
    private readonly TodoRepository _repository;

    public TodoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TodoDbContext(options);
        _repository = new TodoRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // GetAllAsync Tests
    [Fact]
    public async Task GetAllAsync_WhenNoTodos_ReturnsEmptyCollection()
    {
        // Arrange
        // (empty database)

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WhenTodosExist_ReturnsAllTodos()
    {
        // Arrange
        var todo1 = new Todo { Title = "Task 1", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        var todo2 = new Todo { Title = "Task 2", IsCompleted = true, CreatedAt = DateTime.UtcNow };
        _context.Todos.AddRange(todo1, todo2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_WhenTodosExist_ReturnsCorrectTodos()
    {
        // Arrange
        var expectedTitle = "Task 1";
        var todo = new Todo { Title = expectedTitle, IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Contains(result, t => t.Title == expectedTitle);
    }

    // GetByIdAsync Tests
    [Fact]
    public async Task GetByIdAsync_WhenTodoExists_ReturnsTodo()
    {
        // Arrange
        var todo = new Todo { Title = "Task 1", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(todo.Id);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTodoExists_ReturnsCorrectTodo()
    {
        // Arrange
        var expectedTitle = "Task 1";
        var todo = new Todo { Title = expectedTitle, IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(todo.Id);

        // Assert
        Assert.Equal(expectedTitle, result!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTodoDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    // CreateAsync Tests
    [Fact]
    public async Task CreateAsync_WhenValidTodo_ReturnsTodoWithId()
    {
        // Arrange
        var todo = new Todo { Title = "New Task", IsCompleted = false, CreatedAt = DateTime.UtcNow };

        // Act
        var result = await _repository.CreateAsync(todo);

        // Assert
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task CreateAsync_WhenValidTodo_TodoIsPersisted()
    {
        // Arrange
        var todo = new Todo { Title = "New Task", IsCompleted = false, CreatedAt = DateTime.UtcNow };

        // Act
        var result = await _repository.CreateAsync(todo);

        // Assert
        var savedTodo = await _context.Todos.FindAsync(result.Id);
        Assert.NotNull(savedTodo);
    }

    [Fact]
    public async Task CreateAsync_WhenValidTodo_ReturnsCorrectTitle()
    {
        // Arrange
        var expectedTitle = "New Task";
        var todo = new Todo { Title = expectedTitle, IsCompleted = false, CreatedAt = DateTime.UtcNow };

        // Act
        var result = await _repository.CreateAsync(todo);

        // Assert
        Assert.Equal(expectedTitle, result.Title);
    }

    [Fact]
    public async Task CreateAsync_WhenValidTodo_ReturnsCorrectIsCompleted()
    {
        // Arrange
        var todo = new Todo { Title = "New Task", IsCompleted = true, CreatedAt = DateTime.UtcNow };

        // Act
        var result = await _repository.CreateAsync(todo);

        // Assert
        Assert.True(result.IsCompleted);
    }

    // UpdateAsync Tests
    [Fact]
    public async Task UpdateAsync_WhenTodoExists_ReturnsUpdatedTodo()
    {
        // Arrange
        var todo = new Todo { Title = "Original", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        var updatedTodo = new Todo { Title = "Updated", IsCompleted = true, CreatedAt = todo.CreatedAt };

        // Act
        var result = await _repository.UpdateAsync(todo.Id, updatedTodo);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenTodoExists_UpdatesTitle()
    {
        // Arrange
        var todo = new Todo { Title = "Original", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        var expectedTitle = "Updated";
        var updatedTodo = new Todo { Title = expectedTitle, IsCompleted = true, CreatedAt = todo.CreatedAt };

        // Act
        var result = await _repository.UpdateAsync(todo.Id, updatedTodo);

        // Assert
        Assert.Equal(expectedTitle, result!.Title);
    }

    [Fact]
    public async Task UpdateAsync_WhenTodoExists_UpdatesIsCompleted()
    {
        // Arrange
        var todo = new Todo { Title = "Original", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        var updatedTodo = new Todo { Title = "Updated", IsCompleted = true, CreatedAt = todo.CreatedAt };

        // Act
        var result = await _repository.UpdateAsync(todo.Id, updatedTodo);

        // Assert
        Assert.True(result!.IsCompleted);
    }

    [Fact]
    public async Task UpdateAsync_WhenTodoExists_TodoIsPersisted()
    {
        // Arrange
        var todo = new Todo { Title = "Original", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        var expectedTitle = "Updated";
        var updatedTodo = new Todo { Title = expectedTitle, IsCompleted = true, CreatedAt = todo.CreatedAt };

        // Act
        await _repository.UpdateAsync(todo.Id, updatedTodo);

        // Assert
        var savedTodo = await _context.Todos.FindAsync(todo.Id);
        Assert.Equal(expectedTitle, savedTodo!.Title);
    }

    [Fact]
    public async Task UpdateAsync_WhenTodoDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentId = 999;
        var updatedTodo = new Todo { Title = "Updated", IsCompleted = true, CreatedAt = DateTime.UtcNow };

        // Act
        var result = await _repository.UpdateAsync(nonExistentId, updatedTodo);

        // Assert
        Assert.Null(result);
    }

    // DeleteAsync Tests
    [Fact]
    public async Task DeleteAsync_WhenTodoExists_ReturnsTrue()
    {
        // Arrange
        var todo = new Todo { Title = "Task to Delete", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(todo.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenTodoExists_TodoIsRemoved()
    {
        // Arrange
        var todo = new Todo { Title = "Task to Delete", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(todo.Id);

        // Assert
        var deletedTodo = await _context.Todos.FindAsync(todo.Id);
        Assert.Null(deletedTodo);
    }

    [Fact]
    public async Task DeleteAsync_WhenTodoDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }
}
