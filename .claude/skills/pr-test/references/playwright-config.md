# Playwright MCP Configuration

## Server Flags

The Playwright MCP server is configured in `.mcp.json` with flags that apply to every session:

| Flag | Value | Purpose |
|------|-------|---------|
| `--headless` | (boolean) | No visible browser window — faster, no focus issues |
| `--save-video` | `1920x1080` | Automatic video recording at full HD |
| `--viewport-size` | `1920x1080` | Full desktop layout (Umbraco backoffice sidebar visible) |
| `--save-trace` | (boolean) | Playwright trace files for interactive debugging |
| `--output-dir` | `.playwright-mcp` | Evidence directory for video and trace files |
| `--isolated` | (boolean) | Clean browser profile each time (no stale cookies/state) |

### No Manual Configuration Needed

Because `.mcp.json` handles headless mode, viewport size, and video recording, Step 7 of the test pipeline only needs to confirm the browser is ready with a simple `browser_snapshot`. No `browser_run_code` calls are needed to set viewport or detect headless mode.

### Evidence Output

Video and trace files are saved to `.playwright-mcp/` in the repo root. This directory should be gitignored.

To replay a trace interactively:
```bash
npx playwright show-trace .playwright-mcp/trace.zip
```

## Retry Strategy

The Umbraco backoffice is a single-page application with heavy async loading. Elements frequently appear after API calls complete, modals animate in, and tree nodes expand asynchronously. A single attempt at finding or clicking an element will often fail simply due to timing.

### The 3-Attempt Rule

For every browser action (click, verify, fill), follow this pattern:

```
Attempt 1:
  -> browser_snapshot to find the element
  -> Try the action
  -> If success -> move on
  -> If element not found or action fails -> wait 2 seconds

Attempt 2:
  -> browser_snapshot again (DOM may have changed)
  -> Try the action
  -> If success -> move on
  -> If fails -> wait 3 seconds

Attempt 3 (final):
  -> browser_snapshot one more time
  -> Try the action
  -> If success -> move on
  -> If fails -> mark as FAIL with evidence
```

### When to Retry

Retry on these conditions:
- **Element not found** in the accessibility snapshot — it may still be loading
- **Click didn't produce expected result** — the target may not have been interactive yet
- **Page still loading** — async content hasn't rendered
- **Modal hasn't appeared** — animations take time

### When NOT to Retry

Don't retry if:
- **Console shows a JavaScript error** related to the action — this is a real bug, not a timing issue
- **HTTP 4xx/5xx error** in network requests — the server rejected the request
- **The element IS present but in wrong state** — e.g., text says "Error" instead of "Success"

### Wait Strategies Between Retries

In order of preference:

1. **`browser_wait_for` with specific text** — wait for the expected element/text to appear (best, most targeted)
2. **`browser_wait_for` with time: 2** — wait 2 seconds (simple, reliable for most cases)
3. **`browser_wait_for` with textGone** — wait for a loading indicator to disappear

## Timeouts

- **Page navigation**: wait up to 30 seconds for the page to load
- **Element appearance**: wait up to 10 seconds (3 attempts x ~3 seconds each)
- **API responses**: if using `browser_evaluate` for API calls, allow 15 seconds
- **File uploads**: allow 10 seconds for upload + processing
