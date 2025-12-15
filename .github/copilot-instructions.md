# Umbraco CMS Development Guide

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

Bootstrap, build, and test the repository:

-   Install .NET SDK (version specified in global.json):
    -   `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version $(jq -r '.sdk.version' global.json)`
    -   `export PATH="/home/runner/.dotnet:$PATH"`
-   Install Node.js (version specified in src/Umbraco.Web.UI.Client/.nvmrc):
    -   `curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.0/install.sh | bash`
    -   `export NVM_DIR="$HOME/.nvm" && [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"`
    -   `nvm install $(cat src/Umbraco.Web.UI.Client/.nvmrc) && nvm use $(cat src/Umbraco.Web.UI.Client/.nvmrc)`
-   Fix shallow clone issue (required for GitVersioning):
    -   `git fetch --unshallow`
-   Restore packages:
    -   `dotnet restore` -- takes 50 seconds. NEVER CANCEL. Set timeout to 90+ seconds.
-   Build the solution:
    -   `dotnet build` -- takes 4.5 minutes. NEVER CANCEL. Set timeout to 10+ minutes.
-   Install and build frontend:
    -   `cd src/Umbraco.Web.UI.Client`
    -   `npm ci --no-fund --no-audit --prefer-offline` -- takes 11 seconds.
    -   `npm run build:for:cms` -- takes 1.25 minutes. NEVER CANCEL. Set timeout to 5+ minutes.
-   Install and build Login
    -   `cd src/Umbraco.Web.UI.Login`
    -   `npm ci --no-fund --no-audit --prefer-offline`
    -   `npm run build`
-   Run the application:
    -   `cd src/Umbraco.Web.UI`
    -   `dotnet run --no-build` -- Application runs on https://localhost:44339 and http://localhost:11000

Check out [BUILD.md](./BUILD.md) for more detailed instructions.

## Validation

-   ALWAYS run through at least one complete end-to-end scenario after making changes.
-   Build and unit tests must pass before committing changes.
-   Frontend build produces output in src/Umbraco.Web.UI.Client/dist-cms/ which gets copied to src/Umbraco.Web.UI/wwwroot/umbraco/backoffice/
-   Always run `dotnet build` and `npm run build:for:cms` before running the application to see your changes.
-   For login-only changes, you can run `npm run build` from src/Umbraco.Web.UI.Login and then `dotnet run --no-build` from src/Umbraco.Web.UI.
-   For frontend-only changes, you can run `npm run dev:server` from src/Umbraco.Web.UI.Client for hot reloading.
-   Frontend changes should be linted using `npm run lint:fix` which uses Eslint.

## Testing

### Unit Tests (.NET)

-   Location: tests/Umbraco.Tests.UnitTests/
-   Run: `dotnet test tests/Umbraco.Tests.UnitTests/Umbraco.Tests.UnitTests.csproj --configuration Release --verbosity minimal`
-   Duration: ~1 minute with 3,343 tests
-   NEVER CANCEL: Set timeout to 5+ minutes

### Integration Tests (.NET)

-   Location: tests/Umbraco.Tests.Integration/
-   Run: `dotnet test tests/Umbraco.Tests.Integration/Umbraco.Tests.Integration.csproj --configuration Release --verbosity minimal`
-   NEVER CANCEL: Set timeout to 10+ minutes

### Frontend Tests

-   Location: src/Umbraco.Web.UI.Client/
-   Run: `npm test` (requires `npx playwright install` first)
-   Frontend tests use Web Test Runner with Playwright

### Acceptance Tests (E2E)

-   Location: tests/Umbraco.Tests.AcceptanceTest/
-   Requires running Umbraco application and configuration
-   See tests/Umbraco.Tests.AcceptanceTest/README.md for detailed setup (requires `npx playwright install` first)

## Project Structure

The solution contains 30 C# projects organized as follows:

### Main Application Projects

-   **Umbraco.Web.UI**: Main web application project (startup project)
-   **Umbraco.Web.UI.Client**: TypeScript frontend (backoffice)
-   **Umbraco.Web.UI.Login**: Separate login screen frontend
-   **Umbraco.Core**: Core domain models and interfaces
-   **Umbraco.Infrastructure**: Data access and infrastructure
-   **Umbraco.Cms**: Main CMS package

### API Projects

-   **Umbraco.Cms.Api.Management**: Management API
-   **Umbraco.Cms.Api.Delivery**: Content Delivery API
-   **Umbraco.Cms.Api.Common**: Shared API components

### Persistence Projects

-   **Umbraco.Cms.Persistence.SqlServer**: SQL Server support
-   **Umbraco.Cms.Persistence.Sqlite**: SQLite support
-   **Umbraco.Cms.Persistence.EFCore**: Entity Framework Core abstractions

### Test Projects

-   **Umbraco.Tests.UnitTests**: Unit tests
-   **Umbraco.Tests.Integration**: Integration tests
-   **Umbraco.Tests.AcceptanceTest**: End-to-end tests with Playwright
-   **Umbraco.Tests.Common**: Shared test utilities

## Common Tasks

### Running Umbraco in Different Modes

**Production Mode (Standard Development)**
Use this for backend development, testing full builds, or when you don't need hot reloading:

1. Build frontend assets: `cd src/Umbraco.Web.UI.Client && npm run build:for:cms`
2. Run backend: `cd src/Umbraco.Web.UI && dotnet run --no-build`
3. Access backoffice: `https://localhost:44339/umbraco`
4. Application uses compiled frontend from `wwwroot/umbraco/backoffice/`

**Vite Dev Server Mode (Frontend Development with Hot Reload)**
Use this for frontend-only development with hot module reloading:

1. Configure backend for frontend development - Add to `src/Umbraco.Web.UI/appsettings.json` under `Umbraco:CMS:Security`:
    ```json
    "BackOfficeHost": "http://localhost:5173",
    "AuthorizeCallbackPathName": "/oauth_complete",
    "AuthorizeCallbackLogoutPathName": "/logout",
    "AuthorizeCallbackErrorPathName": "/error",
    "BackOfficeTokenCookie": {
      "SameSite": "None"
    }
    ```
2. Run backend: `cd src/Umbraco.Web.UI && dotnet run --no-build`
3. Run frontend dev server: `cd src/Umbraco.Web.UI.Client && npm run dev:server`
4. Access backoffice: `http://localhost:5173/` (no `/umbraco` prefix)
5. Changes to TypeScript/Lit files hot reload automatically

**Important:** Remove the `BackOfficeHost` configuration before committing or switching back to production mode.

### Backend-Only Development

For backend-only changes, disable frontend builds:

-   Comment out the target named "BuildStaticAssetsPreconditions" in src/Umbraco.Cms.StaticAssets.csproj:
    ```
    <!--<Target Name="BuildStaticAssetsPreconditions" BeforeTargets="AssignTargetPaths">
      [...]
    </Target>-->
    ```
-   Remember to uncomment before committing

### Building NuGet Packages

To build custom NuGet packages for testing:

```bash
dotnet pack -c Release -o Build.Out
dotnet nuget add source [Path to Build.Out folder] -n MyLocalFeed
```

### Regenerating Frontend API Types

When changing Management API:

```bash
cd src/Umbraco.Web.UI.Client
npm run generate:server-api-dev
```

Also update OpenApi.json from /umbraco/swagger/management/swagger.json

## Database Setup

Default configuration supports SQLite for development. For production-like testing:

-   Use SQL Server/LocalDb for better performance
-   Configure connection string in src/Umbraco.Web.UI/appsettings.json

## Clean Up / Reset

To reset development environment:

```bash
# Remove configuration and database
rm src/Umbraco.Web.UI/appsettings.json
rm -rf src/Umbraco.Web.UI/umbraco/Data

# Full clean (removes all untracked files)
git clean -xdf .
```

## Version Information

-   Target Framework: .NET (version specified in global.json)
-   Current Version: (specified in version.json)
-   Node.js Requirement: (specified in src/Umbraco.Web.UI.Client/.nvmrc)
-   npm Requirement: Latest compatible version

## Known Issues

-   Build requires full git history (not shallow clone) due to GitVersioning
-   Some NuGet package security warnings are expected (SixLabors.ImageSharp vulnerabilities)
-   Frontend tests require Playwright browser installation: `npx playwright install`
-   Older Node.js versions may show engine compatibility warnings (check .nvmrc for current requirement)

## Timing Expectations

**NEVER CANCEL** these operations - they are expected to take time:

| Operation               | Expected Time | Timeout Setting |
| ----------------------- | ------------- | --------------- |
| `dotnet restore`        | 50 seconds    | 90+ seconds     |
| `dotnet build`          | 4.5 minutes   | 10+ minutes     |
| `npm ci`                | 11 seconds    | 30+ seconds     |
| `npm run build:for:cms` | 1.25 minutes  | 5+ minutes      |
| `npm test`              | 2 minutes     | 5+ minutes      |
| `npm run lint`          | 1 minute      | 5+ minutes      |
| Unit tests              | 1 minute      | 5+ minutes      |
| Integration tests       | Variable      | 10+ minutes     |

Always wait for commands to complete rather than canceling and retrying.
