using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using PlaywrightMcpExploration.Web.Models;

namespace PlaywrightMcpExploration.Tests.Models;

public class TodoTests
{
    [Fact]
    public void Todo_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var todo = new Todo
        {
            Id = 1,
            Title = "Test Todo",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        todo.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Todo_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var id = 1;
        var title = "Test Todo";
        var isCompleted = true;
        var createdAt = DateTime.UtcNow;

        // Act
        var todo = new Todo
        {
            Id = id,
            Title = title,
            IsCompleted = isCompleted,
            CreatedAt = createdAt
        };

        // Assert
        todo.Id.Should().Be(id);
        todo.Title.Should().Be(title);
        todo.IsCompleted.Should().Be(isCompleted);
        todo.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Todo_Title_ShouldBeRequired()
    {
        // Arrange
        var todo = new Todo
        {
            Id = 1,
            Title = null!,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(todo);
        var isValid = Validator.TryValidateObject(todo, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(v => v.MemberNames.Contains("Title"));
    }

    [Fact]
    public void Todo_Title_ShouldNotExceed200Characters()
    {
        // Arrange
        var todo = new Todo
        {
            Id = 1,
            Title = new string('a', 201),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(todo);
        var isValid = Validator.TryValidateObject(todo, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(v => v.MemberNames.Contains("Title"));
    }

    [Fact]
    public void Todo_Title_ShouldAccept200Characters()
    {
        // Arrange
        var todo = new Todo
        {
            Id = 1,
            Title = new string('a', 200),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(todo);
        var isValid = Validator.TryValidateObject(todo, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeTrue();
    }
}
