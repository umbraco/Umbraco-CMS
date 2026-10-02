# GitHub Codespaces / Devcontainer

The Umbraco source code can be edited and debugged directly in the browser via GitHub Codespaces, or locally using VS Code with the Dev Containers extension.

This environment comes with all the tools you need: .NET 10, Node.js 22, SQLite, SMTP4Dev, and pre-configured debug launch profiles.

---

## Quick Start

1. Open the **Run and Debug** panel (Ctrl+Shift+D / Cmd+Shift+D)
2. Select **"Backoffice Launch (Vite + .NET Core)"** from the dropdown
3. Press the green **Play** button (or F5)

This starts both the Vite frontend dev server and the .NET backend simultaneously and attaches the debugger to both. The first run takes longer as it builds and runs the database installer — subsequent runs are fast.

---

## Default Credentials

| Field    | Value               |
|----------|---------------------|
| Username | `test@umbraco.com`  |
| Password | `password1234`      |

The database is auto-installed on first run (unattended mode). No manual setup required.

---

## Debugging

### C# breakpoints

Any of the C# launch profiles will attach the .NET debugger:

- **".NET Core Launch (web)"** — starts the backend only, opens the browser automatically
- **".NET Core Serve with External Auth (web)"** — used by the compound profile; backend configured to serve the Vite frontend

Set a breakpoint in any `.cs` file, press F5, and the debugger will break when that code is hit.

### TypeScript / frontend breakpoints

The **"Backoffice Launch (Vite + .NET Core)"** compound profile also starts the Vite dev server with the Node.js debugger attached. Set a breakpoint in any `.ts` file under `src/Umbraco.Web.UI.Client/src/` and it will be hit in VS Code.

### Frontend only (mock mode)

If you only want to work on the frontend without a running backend, use:

- **"Backoffice Launch Vite (Chrome)"** — prompts whether to use Mock Service Worker (MSW) for API calls

---

## Test Email (SMTP4Dev)

An SMTP4Dev instance runs automatically in the container and catches all outbound emails sent by Umbraco.

- **Web UI**: browse to the forwarded port **5050**
- **SMTP**: port `2525` (already configured via `Umbraco__CMS__Global__Smtp__*` environment variables)

Any email sent by Umbraco (password reset, user invites, etc.) will appear in the SMTP4Dev inbox instead of being delivered.

---

## Ports

| Port  | Service              |
|-------|----------------------|
| 44339 | Umbraco (.NET/HTTPS) |
| 5173  | Backoffice (Vite)    |
| 5050  | SMTP4Dev web UI      |
| 2525  | SMTP4Dev SMTP        |

---

## SQLite Database

The SQLite VS Code extension is pre-installed. You can open, query, and inspect the Umbraco database directly in VS Code:

- File: `src/Umbraco.Web.UI/umbraco/Data/Umbraco.sqlite.db`

To reset the database and start fresh:

```bash
rm -rf src/Umbraco.Web.UI/umbraco/Data
```

---

## Running Tests

Both test suites work out of the box inside the devcontainer:

```bash
# .NET unit and integration tests
dotnet test

# Frontend unit tests
cd src/Umbraco.Web.UI.Client
npm test
```

---

## AI-Assisted Development

This devcontainer is designed to also work with AI coding agents such as [Claude Code](https://claude.ai/code). An AI agent can:

- Build the project: `dotnet build`
- Run the site: `dotnet run --project src/Umbraco.Web.UI`
- Run tests to verify changes: `dotnet test` / `npm test`
- Inspect the SQLite database to check data state
- Watch for compilation errors: `dotnet watch --project src/Umbraco.Web.UI`

The unattended installer, pre-configured connection string, and environment variables mean the agent does not need to perform any manual setup — the site starts and is fully functional on the first `dotnet run`.

---

## Troubleshooting

**First run is slow** — the initial build compiles the full .NET solution and the frontend. Subsequent runs use cached output and are much faster. In GitHub Codespaces with prebuilds enabled, the build runs in the background before you open the Codespace.

**OAuth / login not working in browser-based Codespaces** — the `postCreate.sh` script auto-configures `appsettings.Development.json` with the correct forwarded URLs for your Codespace. If you delete this file, re-run the script: `bash .devcontainer/postCreate.sh`.

**Frontend changes not reflected** — ensure the Vite dev server is running (the compound debug config starts it automatically). Also check that browser caching is disabled in DevTools → Network → "Disable cache".
