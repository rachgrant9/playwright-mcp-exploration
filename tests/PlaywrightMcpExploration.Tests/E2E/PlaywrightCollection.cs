namespace PlaywrightMcpExploration.Tests.E2E;

/// <summary>
/// Collection definition for Playwright tests.
/// Ensures all tests in this collection share the same PlaywrightFixture instance.
/// </summary>
[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
