# Verification Patterns

## How to verify test outcomes using Playwright MCP

Every verification should be attempted **up to 3 times** before marking as FAIL. The Umbraco backoffice loads content asynchronously, so elements may not appear immediately. See `references/playwright-config.md` for the full retry strategy.

### Element Presence

**"Verify {element} appears" / "Should see {element}"**

1. `browser_snapshot` to get the accessibility tree
2. Search the snapshot text for the expected element or text
3. If found → PASS
4. If not found → `browser_wait_for` time 2, then re-snapshot and try again (up to 3 attempts total)
5. After 3 attempts → FAIL

```
browser_snapshot
→ Look for the element by its text, role, or name in the returned accessibility tree
```

### Element Absence

**"Verify {element} does NOT appear" / "Should not see {element}"**

1. `browser_snapshot` to get the accessibility tree
2. Search the snapshot text — the element should NOT be present
3. If not found → PASS. If found → FAIL.

### Text Content

**"Verify {text} shows" / "Check that {text} is displayed"**

1. `browser_snapshot` — look for the exact text in the tree
2. Or `browser_evaluate` with `document.body.innerText.includes('{text}')`

### Element State

**"Verify button is disabled" / "Check that field is readonly"**

1. `browser_snapshot` — look for `disabled` attribute in the element description
2. Elements with `disabled` state show as `(disabled)` in the accessibility tree

### Form Values

**"Verify the field contains {value}"**

1. `browser_snapshot` — input elements show their current value
2. Or `browser_evaluate` to read the value: `document.querySelector('input[name="..."]').value`

### Link Behavior

**"Verify {text} is a clickable link"**

1. `browser_snapshot` — look for `link` role with the expected text
2. Versus just `text` or `paragraph` for non-clickable text

### Modal/Dialog

**"Verify modal opens" / "Verify dialog appears"**

1. `browser_snapshot` — look for `dialog` role in the tree
2. The modal content will be nested inside the dialog element

**"Verify modal closes" / "Pressing ESC dismisses"**

1. `browser_press_key` with key "Escape"
2. `browser_snapshot` — verify no `dialog` role exists

### Navigation

**"Verify page navigates to {destination}"**

1. After the action, check the current URL or page content
2. `browser_snapshot` — look for content specific to the destination page

### Error States

**"Verify validation error appears"**

1. `browser_snapshot` — look for elements with `alert` role or error-styled text
2. Validation messages in Umbraco appear near the invalid field

**"Verify no errors"**

1. `browser_console_messages` with level "error" — should return empty
2. `browser_snapshot` — no error alerts or error-styled elements

### Visual Changes

**"Verify the icon changed" / "Verify the layout looks different"**

These are subjective and hard to verify programmatically. Strategy:
1. Take a screenshot for human review
2. Use `browser_snapshot` to check structural properties (element class, aria-label)
3. Mark as `NEEDS_HUMAN_REVIEW` if the verification is purely visual

### Counts and Lists

**"Verify {N} items appear" / "Verify pagination shows"**

1. `browser_snapshot` — count the relevant elements in the tree
2. Or `browser_evaluate` to count: `document.querySelectorAll('.item-class').length`

### File Upload Results

**"Verify file uploaded successfully"**

1. After `browser_file_upload`, wait briefly
2. `browser_snapshot` — look for the filename or a preview/thumbnail
3. Check for success toast notification

### Tooltip/Hover

**"Verify tooltip appears on hover"**

1. `browser_hover` over the target element
2. Wait briefly (tooltips have a delay)
3. `browser_snapshot` — look for `tooltip` role or newly appeared text

### Drag and Drop

**"Verify drag and drop works"**

1. This is complex with MCP tools. Use `browser_evaluate` to trigger drag events programmatically if `browser_drag` (from the MCP tools) isn't available.
2. Alternatively, report as `NEEDS_MANUAL_TEST` if drag-and-drop is too complex for automation.

## General Strategy

1. **Retry up to 3 times** — the backoffice is async-heavy; give every action a fair chance before failing
2. **Prefer `browser_snapshot`** for structural verification — it's fast and reliable
3. **Re-snapshot before each retry** — the DOM changes between attempts
4. **Use `browser_take_screenshot`** for evidence — reviewers want to see the UI
5. **Use `browser_evaluate`** for complex checks that the snapshot can't express
6. **Wait after actions** — use `browser_wait_for` with expected text or a short delay (2-3 seconds)
7. **Be specific in assertions** — "element with text 'Save' and role 'button'" is better than "Save appears somewhere"
8. **Check console errors** after every major action — they catch issues the UI might hide
9. **Distinguish timing failures from real bugs** — if retries eventually succeed, it's timing; if console errors appear, it's a bug
