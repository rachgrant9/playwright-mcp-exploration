#!/bin/bash

# Quick script to run E2E tests

echo "Running Playwright E2E Tests..."
echo ""

# Check if Playwright browsers are installed
if [ ! -d "$HOME/.cache/ms-playwright" ] && [ ! -d "$HOME/Library/Caches/ms-playwright" ]; then
    echo "⚠️  Playwright browsers not found!"
    echo "Installing Playwright browsers first..."
    ./install-playwright.sh
    echo ""
fi

# Run E2E tests
echo "Starting E2E test suite..."
dotnet test --filter "FullyQualifiedName~E2E" --logger "console;verbosity=normal"

echo ""
echo "Tests completed!"
