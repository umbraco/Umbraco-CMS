# Umbraco Acceptance Tests — Contributor & Agent Guide

Playwright end-to-end tests for Umbraco CMS. This file covers **how the suite is built and the conventions that keep it deterministic**. For setup, running, and writing your first test, see [README.md](./README.md) first — this document does not repeat it.

---

## 1. Type-checking

`npm run build` compiles `lib/` only (`tsconfig.build.json`), so it does **not** type-check the spec files under `tests/`. To catch a type error in a spec, run `tsc` against the root config yourself before committing — it needs no Umbraco instance:

```bash
npx tsc -p tsconfig.json --noEmit   # type-checks lib/ AND tests/
```

There is no lint step: the dropped-promise discipline in §3 is a convention you must apply by hand, not something a tool enforces.

---

## 2. Architecture

Three layers, each building on the one below:

```
BasePage            lib/helpers/BasePage.ts        low-level wrappers: click, enterText, isVisible, hasText, waitForVisible, …
  └─ UiBaseLocators lib/helpers/UiBaseLocators.ts  shared locators + composed UI actions + the response-waiting helpers
       └─ <X>UiHelper  lib/helpers/<X>UiHelper.ts  page-object per area (Content, Library, Media, Member, …)

ApiHelpers          lib/helpers/ApiHelpers.ts      HTTP transport (get/post/put/delete) against the Management API
  └─ <X>ApiHelper   lib/helpers/<X>ApiHelper.ts    per-entity API setup/teardown (Document, Media, Webhook, User, …)
```

- Tests reach these via the `umbracoUi.*` and `umbracoApi.*` fixtures.
- `lib/` is published to npm as `@umbraco/acceptance-test-helpers`. Treat exported helper signatures as a **public contract** — prefer adding optional parameters or new overloads over changing/removing existing ones.

---

## 3. Flakiness-avoidance conventions

These are the rules that keep the suite stable. Most flaky failures trace back to breaking one of them.

### Never drop a promise
Every Playwright assertion (`expect(locator).toBeVisible()`, `.toHaveText()`, …) and every helper call is async. Always `await` it, and **never** wrap awaited work in `Array.forEach(async …)` — the callback's promise is discarded and the assertion never runs.

```ts
// ✗ assertion is dropped — test greens even if it fails
values.forEach(async v => { await ui.doesRenderValueContainText(v); });

// ✓
for (const v of values) { await ui.doesRenderValueContainText(v); }
```

### Wait on state, not on time
Fixed sleeps (`page.waitForTimeout(...)`, `ConstantHelper.wait.*`) are the single largest flakiness source — too short fails under CI load, too long wastes wall-clock. Prefer a deterministic wait:

- **Value waits** — poll a getter until it is truthy:
  ```ts
  await expect.poll(() => api.isDocumentPublished(id), { timeout: ConstantHelper.timeout.veryLong }).toBeTruthy();
  ```
- **Action + assertion retries** — retry a flow that can lose an early click:
  ```ts
  await expect(async () => {
    if (!(await modal.isVisible())) { await this.click(openBtn); }
    await expect(modal).toBeVisible({ timeout: ConstantHelper.timeout.short });
  }).toPass({ timeout: ConstantHelper.timeout.medium });
  ```
- **API round-trips** — use `waitForResponseAfterExecutingPromise` (below), not a sleep after the click.

### Waiting for API responses
`UiBaseLocators.waitForResponseAfterExecutingPromise(url, promise, statusCode)` resolves on the first response whose URL **contains** `url` and whose status matches.

- The match is a substring, so endpoint constants are prefixes of each other: `apiEndpoints.document` also matches `/document-type`, `/document-blueprint`, `/document/{id}/publish`. When more than one such call is in flight, pass a more specific `url` fragment to disambiguate.
- **"Save and publish" hits different endpoints for new vs existing content**: a new document publishes via `POST /document/create-and-publish` (201), an existing one via `PUT /document/{id}/update-and-publish` (200), and the workspace then re-fetches `GET /document/{id}/published`. Waiting on the generic `apiEndpoints.document` (status 200) resolves on whichever 200 `/document` response follows the click, so the same helper works for both cases — don't narrow it to a single publish sub-endpoint.

### `this.click()` vs raw `.click()`
`BasePage.click()` waits for the element to be visible before clicking. Use it. Reach for a raw locator `.click()` only when you deliberately need to skip that wait.

### Match names exactly to survive leftover data
Playwright locators run in **strict mode** — if a locator resolves to more than one element, the action throws. Residue from a crashed/partial run (e.g. a leftover `TestUserGroupNameDescription`) makes a substring locator for `TestUserGroupName` match two rows and fail. Match on exact text whenever the value is an entity name:

```ts
// ✗ also matches 'TestUserGroupNameDescription' → strict-mode multi-match
this.page.locator('umb-user-group-ref', {hasText: name});

// ✓ exact
this.page.locator('umb-user-group-ref').filter({has: this.page.getByText(name, {exact: true})});
```
This is why `UserGroupUiHelper`/`UserUiHelper` filter with `getByText(name, {exact: true})` rather than `hasText`.

---

## 4. Test data & isolation

- **Idempotent cleanup**: create with the API, tear down with `ensureNameNotExists()` in both `beforeEach` and `afterEach`.
- **Tests run serially** (`workers: 1`) because specs share fixed entity names (`TestContent`, …) and would collide in parallel. If you add data, keep names unique to your file/test so cleanup can't affect another test.
- Cleanup caveats to be aware of when debugging leftover state: some `ensureNameNotExists` / `recurseChildren` helpers delete only the **first** match, and list fetches use a single large `take` (no pagination) — duplicates or very large trees can leave residue.

---

## 5. Generated / ignored files

- `console-errors.json` is generated at install/run time and is **git-ignored** — do not commit it. Console errors captured during a run are appended here for inspection; they are not (yet) a failing gate.
- `.env`, `playwright/.auth/`, and `results/` are also ignored.
