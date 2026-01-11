# E2E Tests with Playwright

This folder contains end-to-end tests for the Todo API application using Playwright.

## Prerequisites

Before running the E2E tests, you need to install Playwright browsers:

```bash
# Navigate to the test project directory
cd tests/PlaywrightMcpExploration.Tests

# Build the project
dotnet build

# Install Playwright browsers (choose one of the following methods)

# Method 1: Using PowerShell (Windows/macOS/Linux with PowerShell)
pwsh bin/Debug/net10.0/playwright.ps1 install

# Method 2: Using dotnet playwright (if playwright CLI is installed)
dotnet tool install --global Microsoft.Playwright.CLI
playwright install

# Method 3: Install only Chromium (recommended for CI/CD)
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

## Running the Tests

The E2E tests automatically start the web application on `http://localhost:5000` before running.

```bash
# Run all E2E tests
dotnet test --filter "FullyQualifiedName~E2E"

# Run a specific test
dotnet test --filter "FullyQualifiedName~Should_Create_New_Todo_Successfully"

# Run with verbose output
dotnet test --filter "FullyQualifiedName~E2E" --logger "console;verbosity=detailed"
```

## Test Structure

### PlaywrightFixture.cs
- Sets up and manages the Playwright browser instance
- Starts the web application on localhost:5000
- Implements `IAsyncLifetime` for proper resource management

### PlaywrightCollection.cs
- xUnit collection definition
- Ensures all tests share the same PlaywrightFixture instance
- Improves test performance by reusing the browser instance

### TodoE2ETests.cs
- Contains all E2E test cases
- Each test creates a new browser page for isolation
- Tests follow the AAA (Arrange-Act-Assert) pattern

## Test Coverage

The E2E tests cover the following scenarios:

1. **Application Loading**
   - ✓ Should load the Todo application successfully
   - ✓ Should display empty state when no todos exist

2. **Create Operations**
   - ✓ Should create a new todo successfully
   - ✓ Should clear input after adding todo

3. **Read Operations**
   - ✓ Should display multiple todos
   - ✓ Should display todos in the UI

4. **Update Operations**
   - ✓ Should toggle todo completion status
   - ✓ Should update UI after toggling

5. **Delete Operations**
   - ✓ Should delete todo successfully
   - ✓ Should show empty state after deleting all todos

6. **Validation**
   - ✓ Should show validation error for empty title
   - ✓ Should show validation error for title too long (>200 chars)
   - ✓ Should clear validation error on input

7. **Complete CRUD Workflow**
   - ✓ Should complete full Create-Read-Update-Delete workflow

8. **UI Behavior**
   - ✓ Should disable add button while submitting

## Debugging Tests

To run tests in headed mode (see the browser):

1. Modify [PlaywrightFixture.cs](PlaywrightFixture.cs#L29-L32):
   ```csharp
   _browser = await _playwright.Chromium.LaunchAsync(new()
   {
       Headless = false  // Change to false
   });
   ```

2. Add slowdown for better visibility:
   ```csharp
   _browser = await _playwright.Chromium.LaunchAsync(new()
   {
       Headless = false,
       SlowMo = 500  // Slow down by 500ms per action
   });
   ```

## Troubleshooting

### Browser not found
If you get an error about Chromium not being found, make sure you've run the Playwright install command:
```bash
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

### Port already in use
If port 5000 is already in use, the tests will fail. Stop any other applications using port 5000:
```bash
# Find process using port 5000
lsof -i :5000

# Kill the process
kill -9 <PID>
```

### Tests are flaky
If tests occasionally fail, try increasing wait timeouts in the test code or ensure your machine has sufficient resources.
