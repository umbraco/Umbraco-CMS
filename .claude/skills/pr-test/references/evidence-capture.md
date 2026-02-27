# Evidence Capture

## Screenshots

Take screenshots at key moments during testing:

### When to Screenshot
- **After login** — confirms the backoffice loaded correctly
- **Before each test step** — shows the starting state
- **After each verification** — shows the result (pass or fail)
- **On any error** — captures the error state for debugging
- **Console errors** — screenshot the page when JS errors are detected

### How to Screenshot
```
browser_take_screenshot with type: "png"
```

Save screenshots to `.playwright-mcp/screenshots/` so all evidence lives together with the video and trace files:
- `.playwright-mcp/screenshots/pr-{NUMBER}-step-{N}-{description}.png`
- Example: `.playwright-mcp/screenshots/pr-21887-step-3-filename-visible.png`
- Example: `.playwright-mcp/screenshots/pr-21887-error-console-js-error.png`

Create the directory before taking the first screenshot:
```bash
mkdir -p .playwright-mcp/screenshots
```

### Full Page vs Viewport
- Default: viewport screenshot (what's visible)
- Use `fullPage: true` when you need to capture content below the fold
- Element screenshots: provide `ref` and `element` to capture a specific component

## Video Recording

Video recording is **automatic**. The `.mcp.json` config passes `--save-video=1920x1080` to the Playwright MCP server at launch time.

### How It Works
- Video recording starts when the MCP server launches the browser
- Video is saved when the browser context closes (session end or MCP server restart)
- Output location: `.playwright-mcp/` in the repo root
- Format: `.webm` video file

### Trace Files
The `.mcp.json` config also passes `--save-trace`, which saves Playwright trace files alongside the video. These are useful for interactive debugging:

```bash
npx playwright show-trace .playwright-mcp/trace.zip
```

Trace files include DOM snapshots, network requests, and console logs at each action — much richer than video alone.

## Console Error Checking

After each major action, check for JavaScript errors:

```
browser_console_messages with level: "error"
```

This returns all console errors. Filter for meaningful errors:
- Ignore known benign warnings (e.g., deprecation notices)
- Flag actual errors (uncaught exceptions, failed API calls)
- Include error text in the test report

## Network Request Checking

Check for failed API calls:

```
browser_network_requests with includeStatic: false
```

Look for:
- HTTP 4xx/5xx responses on API calls
- Failed fetch requests
- Missing resources

## Evidence Storage

All Playwright evidence is saved to `.playwright-mcp/`:
- **Video**: `.playwright-mcp/videos/`
- **Traces**: `.playwright-mcp/traces/`
- **Screenshots**: `.playwright-mcp/screenshots/`
- **Console logs**: `.playwright-mcp/console-*.log`

Use descriptive filenames for screenshots:

```
.playwright-mcp/screenshots/pr-{NUMBER}-login.png
.playwright-mcp/screenshots/pr-{NUMBER}-step-1-navigate.png
.playwright-mcp/screenshots/pr-{NUMBER}-step-2-create.png
.playwright-mcp/screenshots/pr-{NUMBER}-step-3-verify-PASS.png
.playwright-mcp/screenshots/pr-{NUMBER}-step-4-verify-FAIL.png
.playwright-mcp/screenshots/pr-{NUMBER}-console-errors.png
```

## Snapshot vs Screenshot

- **`browser_snapshot`** (accessibility tree): Use for finding elements, checking text content, verifying structure. This is your primary tool for assertions.
- **`browser_take_screenshot`** (image): Use for visual evidence that a human reviewer will look at. This captures what the page actually looks like.

Always use both: snapshot for verification logic, screenshot for evidence.
