#!/bin/bash

# Script to install Playwright browsers for E2E testing

echo "Installing Playwright browsers..."

# Navigate to test project
cd "$(dirname "$0")/tests/PlaywrightMcpExploration.Tests"

# Build the project first
echo "Building test project..."
dotnet build

# Install Chromium browser
echo "Installing Chromium browser..."
if command -v pwsh &> /dev/null; then
    pwsh bin/Debug/net10.0/playwright.ps1 install chromium
else
    echo "PowerShell not found. Trying alternative method..."
    
    # Try using dotnet tool
    if ! command -v playwright &> /dev/null; then
        echo "Installing Playwright CLI..."
        dotnet tool install --global Microsoft.Playwright.CLI
    fi
    
    # Add dotnet tools to PATH if not already there
    export PATH="$PATH:$HOME/.dotnet/tools"
    
    # Install browsers
    if command -v playwright &> /dev/null; then
        playwright install chromium
    else
        echo "Error: Could not install Playwright browsers."
        echo "Please install PowerShell or the Playwright CLI manually."
        exit 1
    fi
fi

echo "✓ Playwright browsers installed successfully!"
echo ""
echo "You can now run E2E tests with:"
echo "  dotnet test --filter \"FullyQualifiedName~E2E\""
