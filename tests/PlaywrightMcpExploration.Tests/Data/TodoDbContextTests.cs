using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PlaywrightMcpExploration.Web.Data;
using PlaywrightMcpExploration.Web.Models;

namespace PlaywrightMcpExploration.Tests.Data;

public class TodoDbContextTests : IDisposable
{
    private readonly TodoDbContext _context;

    public TodoDbContextTests()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new TodoDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void TodoDbContext_ShouldHaveTodosDbSet()
    {
        // Assert
        _context.Todos.Should().NotBeNull();
    }

    [Fact]
    public void TodoDbContext_ShouldAddTodo()
    {
        // Arrange
        var todo = new Todo
        {
            Title = "Test Todo",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.Todos.Add(todo);
        _context.SaveChanges();

        // Assert
        var savedTodo = _context.Todos.FirstOrDefault();
        savedTodo.Should().NotBeNull();
        savedTodo!.Id.Should().BeGreaterThan(0);
        savedTodo.Title.Should().Be("Test Todo");
    }

    [Fact]
    public void TodoDbContext_ShouldEnforceTitleRequired()
    {
        // Arrange
        var todo = new Todo
        {
            Title = null!, // This should violate the required constraint
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Action act = () =>
        {
            _context.Todos.Add(todo);
            _context.SaveChanges();
        };

        // Assert
        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public void TodoDbContext_ShouldAcceptTitleWithinMaxLength()
    {
        // Arrange
        var title = new string('a', 200); // At 200 character limit
        var todo = new Todo
        {
            Title = title,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.Todos.Add(todo);
        _context.SaveChanges();

        // Assert
        var savedTodo = _context.Todos.FirstOrDefault();
        savedTodo.Should().NotBeNull();
        savedTodo!.Title.Length.Should().Be(200);
    }

    [Fact]
    public void TodoDbContext_ShouldRetrieveMultipleTodos()
    {
        // Arrange
        var todo1 = new Todo { Title = "First Todo", CreatedAt = DateTime.UtcNow };
        var todo2 = new Todo { Title = "Second Todo", CreatedAt = DateTime.UtcNow };

        // Act
        _context.Todos.AddRange(todo1, todo2);
        _context.SaveChanges();

        // Assert
        var todos = _context.Todos.ToList();
        todos.Should().HaveCount(2);
        todos.Should().Contain(t => t.Title == "First Todo");
        todos.Should().Contain(t => t.Title == "Second Todo");
    }

    [Fact]
    public void TodoDbContext_ShouldUpdateTodo()
    {
        // Arrange
        var todo = new Todo { Title = "Original Title", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        _context.SaveChanges();

        // Act
        todo.Title = "Updated Title";
        todo.IsCompleted = true;
        _context.SaveChanges();

        // Assert
        var updatedTodo = _context.Todos.Find(todo.Id);
        updatedTodo.Should().NotBeNull();
        updatedTodo!.Title.Should().Be("Updated Title");
        updatedTodo.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void TodoDbContext_ShouldDeleteTodo()
    {
        // Arrange
        var todo = new Todo { Title = "Todo to Delete", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        _context.SaveChanges();
        var todoId = todo.Id;

        // Act
        _context.Todos.Remove(todo);
        _context.SaveChanges();

        // Assert
        var deletedTodo = _context.Todos.Find(todoId);
        deletedTodo.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
