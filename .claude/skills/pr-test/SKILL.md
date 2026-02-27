---
name: pr-test
description: Test an Umbraco CMS pull request end-to-end. Checks out the PR into a worktree, builds with the Clean Starter Kit, runs unattended install, then uses Playwright MCP to verify the PR matches its description — capturing screenshots as evidence. Leaves the instance running so the dev can explore. Use this skill whenever the user says "test PR", "try PR", "run PR", "check PR", "verify PR", or provides a PR number and wants to see it working. Also use when someone says "does this PR work?" or "can you test this?". Use /pr-classify first to pick a good candidate.
---

# PR Test Runner

You test Umbraco CMS pull requests by setting up an isolated environment, running the PR's code, and verifying it against the PR description using browser automation.

## Arguments

Required: A PR number (e.g., `21887`)

## Overview

The test follows this pipeline:
1. **Fetch PR info** — get the PR description and changed files
2. **Create worktree** — isolated checkout of the PR branch
3. **Install Clean Starter Kit** — realistic content out of the box
4. **Configure unattended install** — no wizard, auto-creates database and admin user
5. **Build** — compile the solution (detect if frontend build needed)
6. **Start Umbraco** — run on a unique port
7. **Configure Playwright** — headless browser, ready for automation
8. **Login via browser** — authenticate using Playwright MCP
9. **Execute test steps** — translate PR description into browser actions, retrying up to 3 times per step
10. **Report results** — pass/fail per step, upload evidence to a gist, post summary comment on the PR

The instance is **left running** after testing so the developer can explore it manually.

## Step 1: Fetch PR Info

```bash
gh pr view {PR_NUMBER} --repo umbraco/Umbraco-CMS --json number,title,body,headRefName,baseRefName,changedFiles,additions,deletions,labels
```

Read the PR body and extract the test plan. See `references/pr-test-extraction.md` for how to parse the test steps.

Store the extracted data:
- **Test steps** — ordered list of actions and verifications
- **Preconditions** — what content/config needs to exist
- **Expected outcomes** — what each step should produce

## Step 2: Create Worktree

```bash
cd /Users/philw/Projects/Umbraco-CMS
git fetch origin pull/{PR_NUMBER}/head:pr-{PR_NUMBER}
git worktree add .claude/worktrees/pr-{PR_NUMBER} pr-{PR_NUMBER}
```

If the worktree already exists, ask the user if they want to reuse it or recreate it.

## Step 3: Install Clean Starter Kit

Read `references/appsettings-template.md` for the exact configuration.

```bash
cd /Users/philw/Projects/Umbraco-CMS/.claude/worktrees/pr-{PR_NUMBER}
dotnet add src/Umbraco.Web.UI/Umbraco.Web.UI.csproj package Clean --version 7.0.5
```

This installs the [Clean Starter Kit](https://github.com/prjseal/Clean) which provides blog content types, templates, sample content, and media. Its package migrations run automatically during unattended install because `PackageMigrationsUnattended` defaults to `true`.

## Step 4: Configure Unattended Install

Read `references/appsettings-template.md` for the full configuration template.

Create/overwrite `src/Umbraco.Web.UI/appsettings.Development.json` in the worktree with the unattended install configuration. The key settings are:
- SQLite connection string
- `InstallUnattended: true`
- Admin credentials (email: `admin@test.com`, password: `TestPassword1234!`)

Also ensure the data directory is clean:
```bash
rm -rf .claude/worktrees/pr-{PR_NUMBER}/src/Umbraco.Web.UI/umbraco/Data
```

## Step 5: Build

Read `references/build-detection.md` to determine if a frontend build is needed.

Check if the PR changes frontend files:
```bash
gh pr diff {PR_NUMBER} --repo umbraco/Umbraco-CMS --name-only | grep -c "src/Umbraco.Web.UI.Client/"
```

**Backend only (no frontend changes):**
```bash
cd /Users/philw/Projects/Umbraco-CMS/.claude/worktrees/pr-{PR_NUMBER}
dotnet build src/Umbraco.Web.UI/Umbraco.Web.UI.csproj -c Debug
```

**With frontend changes:**
```bash
cd /Users/philw/Projects/Umbraco-CMS/.claude/worktrees/pr-{PR_NUMBER}/src/Umbraco.Web.UI.Client
npm install
npm run build
cd /Users/philw/Projects/Umbraco-CMS/.claude/worktrees/pr-{PR_NUMBER}
dotnet build src/Umbraco.Web.UI/Umbraco.Web.UI.csproj -c Debug
```

If the build fails, report the error to the user and stop. The PR may have merge conflicts or dependency issues.

## Step 6: Start Umbraco

Calculate port: `PORT = 10000 + ({PR_NUMBER} % 10000)`

Before starting, check the port isn't already in use. If it is, see `references/port-management.md` for how to pick an alternative port or resolve the conflict.

```bash
# Check for port conflict first
lsof -ti:${PORT} 2>/dev/null && echo "WARNING: Port ${PORT} is in use!" || echo "Port ${PORT} is free"
```

```bash
cd /Users/philw/Projects/Umbraco-CMS/.claude/worktrees/pr-{PR_NUMBER}
dotnet run --project src/Umbraco.Web.UI --urls "http://localhost:{PORT}" --no-build &
```

Run this in the background. Then poll until ready:

```bash
for i in $(seq 1 60); do
  status=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:{PORT}/umbraco" 2>/dev/null)
  if [ "$status" = "200" ] || [ "$status" = "302" ]; then
    echo "Umbraco is ready on port {PORT}"
    break
  fi
  echo "Waiting for Umbraco to start... (attempt $i/60)"
  sleep 3
done
```

If it doesn't become ready within 3 minutes, check the logs:
```bash
cat .claude/worktrees/pr-{PR_NUMBER}/src/Umbraco.Web.UI/umbraco/Logs/UmbracoTraceLog.*.json | tail -50
```

## Step 7: Confirm Playwright is Ready

The Playwright MCP server is configured in `.mcp.json` with headless mode, 1920x1080 viewport, and automatic video recording. No manual setup is needed.

Confirm the browser is ready:

```
browser_snapshot
```

If the snapshot succeeds, the browser is running and ready for navigation. Video recording and trace capture are happening automatically in the background — evidence will be saved to `.playwright-mcp/` when the session ends.

See `references/playwright-config.md` for retry strategy and timeout details.

## Step 8: Login via Playwright MCP

Navigate to the backoffice and login:

1. `browser_navigate` to `http://localhost:{PORT}/umbraco`
2. Wait for the login page to load (`browser_wait_for` text "Login" or look for email field)
3. `browser_snapshot` to see the login form
4. `browser_fill_form` or `browser_type` to enter:
   - Email: `admin@test.com`
   - Password: `TestPassword1234!`
5. Click the login/sign-in button
6. `browser_wait_for` the dashboard to load (look for "Content" in the sidebar)
7. Take a screenshot to confirm login succeeded

Read `references/umbraco-navigation.md` for how to navigate the backoffice.

## Step 9: Execute Test Steps

For each test step extracted from the PR:

1. **Take a "before" screenshot** if this is a verification step
2. **Translate the step to browser actions** using the patterns in `references/umbraco-navigation.md`
3. **Execute the browser actions** using Playwright MCP tools
4. **Verify the outcome** using the patterns in `references/verification-patterns.md`
5. **Take an "after" screenshot** as evidence
6. **Record pass/fail** for this step

Read `references/evidence-capture.md` for screenshot and video recording guidance.

### Retry logic

The Umbraco backoffice is a SPA with async loading — elements may not appear immediately after navigation or actions. To give each step a fair chance:

- **Retry each action up to 3 times** before marking it as FAIL
- **Wait 2-3 seconds between retries** to allow async operations to complete
- **Re-snapshot before each retry** — the DOM may have changed
- Only mark a step as FAIL after all 3 attempts have been exhausted

This is especially important for:
- Clicking elements that appear after async loading (modals, pickers, tree expansion)
- Verifying text that renders after an API call completes
- Form submissions that trigger server-side processing

### Key Playwright MCP patterns:

- **Always use `browser_snapshot`** before clicking — it gives you the accessibility tree with element refs
- **Use `browser_take_screenshot`** at key moments for evidence
- **Check `browser_console_messages` with level "error"** after each major action to catch JS errors
- **Use `browser_wait_for`** after navigation or actions that cause page loads

### Handling preconditions:

If the test needs content that doesn't exist:
- **Navigate the backoffice UI** to create it — click through menus, fill forms, save
- The Clean Starter Kit provides blog content types, templates, and media — use these when possible
- If you can't create something via the UI after retries, mark the step as NEEDS_HUMAN_REVIEW

**IMPORTANT — UI only:**
- **Do NOT** use `browser_evaluate` to call the Management API directly
- **Do NOT** modify the database or make direct API calls to set up test data
- **Do NOT** bypass the UI to work around problems — if the UI doesn't work, that's a finding
- All actions must go through the backoffice UI, the same way a real user would

## Step 10: Report Results and Post to PR

After all test steps are complete, do two things: tell the user the results locally, and post evidence to the PR as a comment.

### Evidence directory

Video and trace files are saved automatically to `.playwright-mcp/` by the Playwright MCP server. Screenshots are taken manually during test steps.

### Local report

Present this to the user in the conversation:

```
## PR #{NUMBER}: {TITLE}
**Branch:** {headRefName}
**Status:** PASS / FAIL / PARTIAL
**Evidence:** .claude/pr-test-evidence/pr-{NUMBER}/

### Test Results
| Step | Action | Expected | Result | Evidence |
|------|--------|----------|--------|----------|
| 1 | Navigate to Settings > Data Types | Page loads | PASS | screenshot-1.png |
| 2 | Create upload field data type | Data type created | PASS | screenshot-2.png |
| 3 | Upload image, verify filename | Filename as text | FAIL | screenshot-3.png |

### Console Errors
{list any JS console errors found, or "None"}

### Network Errors
{list any failed API calls, or "None"}

### Notes
{any observations, edge cases found, or areas that need human review}

### Instance Still Running
The Umbraco instance is running at http://localhost:{PORT}/umbraco
Login: admin@test.com / TestPassword1234!
Run /pr-cleanup {PR_NUMBER} when done exploring.
```

### Post evidence to the PR

Read `references/pr-comment-evidence.md` for the full process. In short:

1. **Post a text-only comment** on the PR via `gh pr comment` with:
   - Pass/fail summary table
   - Console/network error summary
   - Notes on edge cases or issues found
2. **Ask the user before posting** — confirm they want the comment added to the PR
3. **Tell the user where local evidence is** — screenshots in the working directory, video/trace in `.playwright-mcp/`
4. **Do NOT attempt to upload images via gists** — `gh gist create` corrupts binary files. If the user wants images on the PR, they can edit the comment on GitHub and drag-drop the screenshot files in.

## Important Notes

- **Leave the instance running** — the developer may want to explore
- **Be thorough with edge cases** — if the PR fixes a specific bug, try to trigger the original bug too
- **Check console errors** — even if the UI looks correct, JS errors indicate problems
- **Take screenshots liberally** — evidence is cheap, reviewers appreciate seeing the actual UI
- **If a step fails, continue testing** — report all results, not just the first failure
- **If you can't figure out how to test something**, mark it as NEEDS_HUMAN_REVIEW rather than skipping silently
