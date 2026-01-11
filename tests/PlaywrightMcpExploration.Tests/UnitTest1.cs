using FluentAssertions;

namespace PlaywrightMcpExploration.Tests;

public class SampleTests
{
    [Fact]
    public void SimpleAssertion_ShouldPass()
    {
        // Arrange
        var expected = 42;
        
        // Act
        var actual = 42;
        
        // Assert
        actual.Should().Be(expected);
    }
    
    [Fact]
    public void StringAssertion_ShouldPass()
    {
        // Arrange
        var greeting = "Hello, World!";
        
        // Act & Assert
        greeting.Should().StartWith("Hello")
            .And.EndWith("!")
            .And.Contain("World");
    }
    
    [Fact]
    public void CollectionAssertion_ShouldPass()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };
        
        // Act & Assert
        numbers.Should().HaveCount(5)
            .And.Contain(3)
            .And.BeInAscendingOrder();
    }
}
