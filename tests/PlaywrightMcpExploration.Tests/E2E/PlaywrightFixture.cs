using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace PlaywrightMcpExploration.Tests.E2E;

/// <summary>
/// Fixture for setting up Playwright browser and web application for E2E tests.
/// Implements IAsyncLifetime to properly initialize and dispose resources.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private WebApplicationFactory<Program>? _factory;

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized");
    public string BaseUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Create web application factory
        _factory = new WebApplicationFactory<Program>();

        // Start the web application - the factory will handle the server
        var client = _factory.CreateClient();
        BaseUrl = _factory.Server.BaseAddress.ToString().TrimEnd('/');

        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
        _factory?.Dispose();
    }
}
