# Posting Test Evidence to the PR

After testing, post a comment on the PR with the results. The comment is text-only — GitHub's API doesn't support uploading images from CLI, so screenshots and video are saved locally for the user to attach manually if desired.

## Step 1: Post the PR Comment

Use `gh pr comment` to post a structured test report:

```bash
gh pr comment {PR_NUMBER} --repo umbraco/Umbraco-CMS --body "$(cat <<'EOF'
## Automated PR Test Results

**Status:** PASS / FAIL / PARTIAL
**Tested by:** Claude Code automated PR tester

### Test Steps
| # | Action | Expected | Result |
|---|--------|----------|--------|
| 1 | Navigate to Settings > Data Types | Page loads | ✅ PASS |
| 2 | Create upload field data type | Data type created | ✅ PASS |
| 3 | Upload image, verify filename | Filename as text | ❌ FAIL |

### Console Errors
None

### Notes
- Step 3 failed: filename did not appear after upload
- All other functionality works as described

### Evidence
Screenshots and video were captured locally. Ask the tester for files if needed.

---
*🤖 This test was run automatically using [Claude Code](https://claude.ai/claude-code) with Playwright MCP against a clean Umbraco instance with the Clean Starter Kit.*
EOF
)"
```

## Step 2: Tell the User Where Evidence Is

After posting the comment, tell the user:

```
All evidence saved to .playwright-mcp/:
  .playwright-mcp/screenshots/pr-{N}-step-*.png
  .playwright-mcp/videos/*.webm
  .playwright-mcp/traces/

To add images to the PR comment:
  1. Open the PR comment on GitHub
  2. Click the pencil icon to edit
  3. Drag-drop the screenshot files into the editor
  GitHub will upload them and insert markdown image links.

To replay the Playwright trace interactively:
  npx playwright show-trace .playwright-mcp/traces/
```

## Why Not Upload Images From CLI?

- **Gists** — `gh gist create` sends file content as JSON strings. Binary files (PNG, WebM) get corrupted because they aren't valid UTF-8.
- **GitHub REST API** — no endpoint for uploading images to PR/issue comments.
- **GitHub web upload** — the drag-and-drop upload uses an internal endpoint that requires browser session auth, not a personal access token.
- **Release assets** — would work technically but creates releases on the upstream repo, which is inappropriate for test evidence.

The simplest reliable approach: post a text summary from CLI, let the user add images manually via the GitHub web UI.

## Comment Format

Keep the comment concise and scannable. Reviewers should be able to glance at it and know:
1. Did it pass or fail?
2. Which steps failed?
3. What was observed?

Use emoji for quick visual scanning:
- ✅ PASS
- ❌ FAIL
- ⚠️ NEEDS_HUMAN_REVIEW
- ⏭️ SKIPPED
