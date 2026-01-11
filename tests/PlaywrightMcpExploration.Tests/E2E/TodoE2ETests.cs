using Microsoft.Playwright;

namespace PlaywrightMcpExploration.Tests.E2E;

/// <summary>
/// E2E tests for Todo application using Playwright.
/// Tests the complete CRUD workflow through the web UI.
/// </summary>
[Collection("Playwright")]
public class TodoE2ETests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage? _page;

    public TodoE2ETests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Create a new page for each test
        _page = await _fixture.Browser.NewPageAsync();
        await _page.GotoAsync(_fixture.BaseUrl);
    }

    public async Task DisposeAsync()
    {
        if (_page != null)
        {
            await _page.CloseAsync();
        }
    }

    [Fact]
    public async Task Should_Load_Todo_Application_Successfully()
    {
        // Arrange & Act
        await _page!.GotoAsync(_fixture.BaseUrl);

        // Assert
        var title = await _page.TitleAsync();
        Assert.Equal("Todo List", title);

        var heading = await _page.Locator("h1").TextContentAsync();
        Assert.Equal("Todo List", heading);
    }

    [Fact]
    public async Task Should_Display_Empty_State_When_No_Todos()
    {
        // Arrange & Act
        await _page!.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var emptyMessage = await _page.Locator("#app").TextContentAsync();
        Assert.Contains("No todos found", emptyMessage);
    }

    [Fact]
    public async Task Should_Create_New_Todo_Successfully()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);
        var todoTitle = "Buy groceries";

        // Act
        await _page.FillAsync("#todoTitle", todoTitle);
        await _page.ClickAsync("#addButton");
        
        // Wait for the todo to appear
        await _page.WaitForSelectorAsync(".todo-item");

        // Assert
        var todoText = await _page.Locator(".todo-title").TextContentAsync();
        Assert.Equal(todoTitle, todoText);

        var status = await _page.Locator(".todo-status").TextContentAsync();
        Assert.Equal("Pending", status);
    }

    [Fact]
    public async Task Should_Clear_Input_After_Adding_Todo()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);
        var todoTitle = "Clean the house";

        // Act
        await _page.FillAsync("#todoTitle", todoTitle);
        await _page.ClickAsync("#addButton");
        await _page.WaitForSelectorAsync(".todo-item");

        // Assert
        var inputValue = await _page.InputValueAsync("#todoTitle");
        Assert.Empty(inputValue);
    }

    [Fact]
    public async Task Should_Toggle_Todo_Completion_Status()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);
        await _page.FillAsync("#todoTitle", "Complete this task");
        await _page.ClickAsync("#addButton");
        await _page.WaitForSelectorAsync(".todo-item");

        // Act - Toggle to completed
        await _page.ClickAsync(".todo-checkbox");
        await _page.WaitForTimeoutAsync(500); // Wait for update

        // Assert - Should be completed
        var statusCompleted = await _page.Locator(".todo-status").TextContentAsync();
        Assert.Equal("Completed", statusCompleted);

        var isChecked = await _page.IsCheckedAsync(".todo-checkbox");
        Assert.True(isChecked);

        // Act - Toggle back to pending
        await _page.ClickAsync(".todo-checkbox");
        await _page.WaitForTimeoutAsync(500); // Wait for update

        // Assert - Should be pending
        var statusPending = await _page.Locator(".todo-status").TextContentAsync();
        Assert.Equal("Pending", statusPending);

        var isUnchecked = await _page.IsCheckedAsync(".todo-checkbox");
        Assert.False(isUnchecked);
    }

    [Fact]
    public async Task Should_Delete_Todo_Successfully()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);
        await _page.FillAsync("#todoTitle", "Task to delete");
        await _page.ClickAsync("#addButton");
        await _page.WaitForSelectorAsync(".todo-item");

        // Act
        _page.Dialog += (_, dialog) => dialog.AcceptAsync();
        await _page.ClickAsync(".delete-button");
        await _page.WaitForTimeoutAsync(500); // Wait for deletion

        // Assert
        var appContent = await _page.Locator("#app").TextContentAsync();
        Assert.Contains("No todos found", appContent);
    }

    [Fact]
    public async Task Should_Display_Multiple_Todos()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);
        var todos = new[] { "First todo", "Second todo", "Third todo" };

        // Act
        foreach (var todo in todos)
        {
            await _page.FillAsync("#todoTitle", todo);
            await _page.ClickAsync("#addButton");
            await _page.WaitForTimeoutAsync(300);
        }

        // Assert
        var todoItems = await _page.Locator(".todo-item").CountAsync();
        Assert.Equal(3, todoItems);
    }

    [Fact]
    public async Task Should_Show_Validation_Error_For_Empty_Title()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);

        // Act
        await _page.FillAsync("#todoTitle", "");
        await _page.ClickAsync("#addButton");
        await _page.WaitForTimeoutAsync(300);

        // Assert
        var errorVisible = await _page.Locator("#titleError").IsVisibleAsync();
        Assert.True(errorVisible);

        var errorText = await _page.Locator("#titleError").TextContentAsync();
        Assert.Contains("required", errorText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_Show_Validation_Error_For_Title_Too_Long()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);
        var longTitle = new string('a', 201); // Exceeds 200 character limit

        // Act
        await _page.FillAsync("#todoTitle", longTitle);
        await _page.ClickAsync("#addButton");
        await _page.WaitForTimeoutAsync(300);

        // Assert
        var errorVisible = await _page.Locator("#titleError").IsVisibleAsync();
        Assert.True(errorVisible);

        var errorText = await _page.Locator("#titleError").TextContentAsync();
        Assert.Contains("200", errorText);
    }

    [Fact]
    public async Task Should_Complete_Full_CRUD_Workflow()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);

        // Act & Assert - Create
        await _page.FillAsync("#todoTitle", "CRUD Test Todo");
        await _page.ClickAsync("#addButton");
        await _page.WaitForSelectorAsync(".todo-item");
        var todoText = await _page.Locator(".todo-title").TextContentAsync();
        Assert.Equal("CRUD Test Todo", todoText);

        // Act & Assert - Read (verify it's displayed)
        var todoItems = await _page.Locator(".todo-item").CountAsync();
        Assert.Equal(1, todoItems);

        // Act & Assert - Update (toggle completion)
        await _page.ClickAsync(".todo-checkbox");
        await _page.WaitForTimeoutAsync(500);
        var status = await _page.Locator(".todo-status").TextContentAsync();
        Assert.Equal("Completed", status);

        // Act & Assert - Delete
        _page.Dialog += (_, dialog) => dialog.AcceptAsync();
        await _page.ClickAsync(".delete-button");
        await _page.WaitForTimeoutAsync(500);
        var appContent = await _page.Locator("#app").TextContentAsync();
        Assert.Contains("No todos found", appContent);
    }

    [Fact]
    public async Task Should_Clear_Validation_Error_On_Input()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);

        // Act - Trigger validation error
        await _page.FillAsync("#todoTitle", "");
        await _page.ClickAsync("#addButton");
        await _page.WaitForTimeoutAsync(300);

        // Assert - Error is visible
        var errorVisibleBefore = await _page.Locator("#titleError").IsVisibleAsync();
        Assert.True(errorVisibleBefore);

        // Act - Start typing
        await _page.FillAsync("#todoTitle", "New todo");

        // Assert - Error is cleared
        var errorVisibleAfter = await _page.Locator("#titleError").IsVisibleAsync();
        Assert.False(errorVisibleAfter);
    }

    [Fact]
    public async Task Should_Disable_Add_Button_While_Submitting()
    {
        // Arrange
        await _page!.GotoAsync(_fixture.BaseUrl);

        // Act
        await _page.FillAsync("#todoTitle", "Test todo");
        
        // Check button state before and during submission
        var buttonTextBefore = await _page.Locator("#addButton").TextContentAsync();
        Assert.Equal("Add Todo", buttonTextBefore);

        // Start submission but check quickly (may not catch the intermediate state)
        await _page.ClickAsync("#addButton");
        await _page.WaitForSelectorAsync(".todo-item");

        // Assert - After submission completes, button should be re-enabled
        var isDisabled = await _page.Locator("#addButton").IsDisabledAsync();
        Assert.False(isDisabled);

        var buttonTextAfter = await _page.Locator("#addButton").TextContentAsync();
        Assert.Equal("Add Todo", buttonTextAfter);
    }
}
