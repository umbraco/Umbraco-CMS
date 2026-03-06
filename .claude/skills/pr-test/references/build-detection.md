# Build Detection

## Determining What Needs Building

Check the PR's changed files to decide the build strategy:

```bash
gh pr diff {PR_NUMBER} --repo umbraco/Umbraco-CMS --name-only
```

### Backend Only (most common)

If changed files are all `.cs`, `.csproj`, `.json` (not in `Umbraco.Web.UI.Client/`), or other non-frontend files:

```bash
cd /path/to/worktree
dotnet build src/Umbraco.Web.UI/Umbraco.Web.UI.csproj -c Debug
```

Typical build time: **1-3 minutes**

### Frontend + Backend

If any changed files match `src/Umbraco.Web.UI.Client/**`:

```bash
cd /path/to/worktree/src/Umbraco.Web.UI.Client
npm install
npm run build
cd /path/to/worktree
dotnet build src/Umbraco.Web.UI/Umbraco.Web.UI.csproj -c Debug
```

Typical build time: **3-8 minutes** (npm install + build adds significant time)

The frontend build produces static assets that are referenced by `Umbraco.Cms.StaticAssets`, which is included in the web project.

### Frontend Only (rare)

If the PR only changes frontend files, you still need `dotnet build` to ensure the static assets are properly included. Always run both.

## Build Failure Handling

If `dotnet build` fails:
1. Check if the PR has merge conflicts with main
2. Check if there are missing package references
3. Look for compiler errors in the output
4. Report the build failure to the user — this likely means the PR needs updating

If `npm install` fails:
1. Try deleting `node_modules` and `package-lock.json` and retrying
2. Check if there's a Node.js version requirement
3. Report the failure to the user

## File Patterns for Detection

| Pattern | Build Type |
|---------|-----------|
| `src/Umbraco.Web.UI.Client/**/*.ts` | Frontend + Backend |
| `src/Umbraco.Web.UI.Client/**/*.js` | Frontend + Backend |
| `src/Umbraco.Web.UI.Client/package.json` | Frontend + Backend |
| `src/**/*.cs` | Backend Only |
| `src/**/*.csproj` | Backend Only |
| `tests/**` | Backend Only (tests don't need to be built for UI testing) |
