# Azure CI/CD Setup Guide

This guide explains how to set up automated deployments to Azure using GitHub Actions.

## Prerequisites

- An Azure Web App resource already created (resource group: `playwright-mcp-rg`, app name: `playwright-mcp-exploration`)
- GitHub repository with admin access
- Azure CLI installed locally (for obtaining credentials)

## Setup Instructions

### 1. Get Azure Publish Profile

You need to obtain the publish profile for your Azure Web App and store it as a GitHub secret.

**Option A: Using Azure Portal**
1. Go to the [Azure Portal](https://portal.azure.com)
2. Navigate to your App Service (`playwright-mcp-exploration`)
3. Click "Get publish profile" in the overview section
4. Save the downloaded `.PublishSettings` file

**Option B: Using Azure CLI**
```bash
az webapp deployment list-publishing-profiles \
  --name playwright-mcp-exploration \
  --resource-group playwright-mcp-rg \
  --xml
```

### 2. Add GitHub Secret

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
5. Value: Paste the entire contents of the publish profile XML
6. Click **Add secret**

### 3. Workflow Overview

The deployment workflow (`.github/workflows/azure-deploy.yml`) consists of two jobs:

#### Build and Deploy Job
- Triggers on push to `main` branch or manual workflow dispatch
- Sets up .NET 10
- Restores dependencies and builds the application
- Runs unit tests to ensure code quality
- Publishes the application
- Deploys to Azure Web App using the publish profile

#### Verify Deployment Job
- Runs after successful deployment
- Waits for the application to be ready
- Runs Playwright E2E tests against the deployed application
- Uploads test results as artifacts
- Ensures deployment is working correctly

### 4. Environment Variables

The workflow uses these environment variables:
- `AZURE_WEBAPP_NAME`: The name of your Azure Web App (`playwright-mcp-exploration`)
- `AZURE_WEBAPP_PACKAGE_PATH`: Path to the .NET project
- `DOTNET_VERSION`: .NET version (`10.0.x`)
- `PLAYWRIGHT_BASE_URL`: Base URL for E2E tests (set automatically)

### 5. Manual Deployment

You can trigger a deployment manually:
1. Go to **Actions** tab in your GitHub repository
2. Select **Deploy to Azure** workflow
3. Click **Run workflow**
4. Select the branch and click **Run workflow**

### 6. Monitoring Deployments

- **GitHub Actions**: View deployment status and logs in the Actions tab
- **Azure Portal**: Monitor application health in the App Service
- **Test Results**: Download Playwright reports from workflow artifacts

## Security Best Practices

✅ **DO:**
- Store publish profile as a GitHub secret (never commit to repository)
- Rotate publish profile periodically
- Use minimal permissions for deployment credentials
- Review deployment logs for security issues

❌ **DON'T:**
- Commit credentials or secrets to the repository
- Share publish profile publicly
- Use the same credentials across multiple environments

## Troubleshooting

### Deployment Fails
- Check GitHub Actions logs for error messages
- Verify Azure Web App is running
- Ensure publish profile is valid and not expired
- Check .NET version compatibility

### E2E Tests Fail After Deployment
- Verify application is accessible at the Azure URL
- Check if database migrations completed successfully
- Review Playwright test artifacts for detailed error information
- Ensure the app has fully started (increase wait time if needed)

### Secret Not Found Error
- Verify secret name matches exactly: `AZURE_WEBAPP_PUBLISH_PROFILE`
- Ensure secret is added to repository (not organization or environment)
- Check repository permissions for GitHub Actions

## Next Steps

Consider adding:
- Staging environment for testing before production
- Database migrations in the deployment pipeline
- Automated rollback on test failures
- Slack/email notifications for deployment status
- Performance monitoring integration
