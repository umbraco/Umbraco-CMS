# Port Management

## Default Port Allocation

By default, each PR gets a unique port based on its number:

```
PORT = 10000 + (PR_NUMBER % 10000)
```

For example:
- PR #21887 → port 11887
- PR #21895 → port 11895
- PR #21830 → port 11830

## Checking for Conflicts

Before starting Umbraco, check if the port is already in use:

```bash
lsof -ti:{PORT} 2>/dev/null
```

If something is already listening on that port, you have two options:

### Option 1: Pick a Different Port

Try adding 20000 instead of 10000:

```bash
PORT=$((20000 + PR_NUMBER % 10000))
```

Or use a completely different base:

```bash
PORT=$((30000 + PR_NUMBER % 10000))
```

Or just find any free port:

```bash
# Find a random free port
PORT=$(python3 -c 'import socket; s=socket.socket(); s.bind(("",0)); print(s.getsockname()[1]); s.close()')
```

### Option 2: Stop the Conflicting Process

If the conflict is from a previous PR test run that wasn't cleaned up:

```bash
# See what's using the port
lsof -i:{PORT}

# Kill it if it's a leftover dotnet process
kill $(lsof -ti:{PORT})
```

## Passing the Port to Umbraco

The port is set via the `--urls` flag:

```bash
dotnet run --project src/Umbraco.Web.UI --urls "http://localhost:{PORT}" --no-build
```

## Remembering the Port

After starting, always tell the user which port the instance is running on. The cleanup skill needs this port to find and stop the process later. The port formula is deterministic from the PR number, so `/pr-cleanup` can recalculate it — but if you used a non-default port, mention it explicitly so the user knows.

## Common Conflicts

| Port Range | Common Use |
|-----------|-----------|
| 5000-5001 | Default ASP.NET Core / Kestrel |
| 8080 | Common dev servers |
| 3000 | Node.js / React dev servers |
| 11000 | Umbraco default (from launchSettings.json) |
| 44339 | Umbraco HTTPS (from launchSettings.json) |

The 10000+ range is generally safe, but if the dev is running multiple PR tests simultaneously or has other services in that range, conflicts can occur.
