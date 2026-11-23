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
- `lib/` is published to npm as `@umbraco-cms/acceptance-test-helpers`. Treat exported helper signatures as a **public contract** — prefer adding optional parameters or new overloads over changing/removing existing ones.

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

- The match is a substring, so endpoint constants are prefixes of each other: `apiEndpoints.document` also matches `/document-type`, `/document-blueprint`, and side requests such as `GET /document/{id}/referenced-by` that the workspace fires while a save is still running. Pass the HTTP `method` and, where possible, a more specific `url` fragment, and pass the action as a callback so the listener is attached before it starts.
- **A save is not finished when its response arrives.** After the update succeeds the workspace re-reads the entity, and until that lands it still reports unsaved changes - navigating away in that window shows the "Discard unsaved changes" dialog. Wait for the workspace action's success state instead (`waitForWorkspaceToBeSaved` / `clickSaveButtonAndWaitForWorkspaceToBeSaved`, built on `waitForWorkspaceActionToSucceed`): the action's button only shows it once the whole save, re-read included, has finished. Element, media and member saves first send `PUT .../{id}/validate` under the same endpoint prefix, so the method alone does not identify the update; `waitForWorkspaceToBeSaved` skips validate responses.
- **"Save and publish" hits different endpoints for new vs existing content** (`POST /document/create-and-publish` 201, `PUT /document/{id}/update-and-publish` 200), so no single response wait fits both. `waitForWorkspaceToBePublished` waits on the publish action's success state instead.

### `this.click()` vs raw `.click()`
`BasePage.click()` waits for the element to be visible before clicking. Use it. Reach for a raw locator `.click()` only when you deliberately need to skip that wait.

The `BasePage` action wrappers (`click`, `enterText`, `hover`, `selectOption`, …) also cap the action itself at `ConstantHelper.timeout.action`, so a stuck action fails with its own error. `actionTimeout` in `playwright.config.ts` stays unlimited on purpose: it would also cap API requests, `waitForResponse` and `waitForEvent`. A raw locator action therefore has no limit of its own and runs until the test timeout.

Avoid `{force: true}` unless the element genuinely cannot pass Playwright's checks: a forced click skips the check that the element is the one under the pointer, so it can land on a neighbour while the layout is still settling.

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

## 5. Authentication

Back-office auth is **cookie-only**. There is no OAuth/PKCE flow, no `oauth_complete` callback and no client-readable token — the httpOnly authentication cookie is the sole credential.

- **Signing in**: `POST security/back-office/login` (`ApiHelpers.signIn` → `LoginApiHelper.login`). The response sets the cookie on the browser context; Playwright's `page.request` shares that cookie jar, so every later API and UI request carries it. Never set an `Authorization` header — `ApiHelpers.getHeaders()` deliberately returns none.
- **Signing out**: `GET security/back-office/signout` (`ApiHelpers.signOut`). `AllowAnonymous`, so it is safe to call without a session. It answers with a redirect to the client logout landing; the cleared cookie is the point, not the redirect.
- **Session lifetime**: `POST security/back-office/keep-alive` renews the cookie, and `GET user/current/configuration` probes it without renewing (a non-ok response means "no session"). `ApiHelpers.isLoginStateValid()` — run before every test by the `umbracoApi` fixture — uses both.
- **Keep the session alive with config, not with the login UI.** Every acceptance-test app sets `Umbraco:CMS:Security:KeepUserLoggedIn: true`, which slides the cookie expiry on every request. A test should never drive the login UI just to dodge an expiring session; drive it only when login itself is what is under test.
- **Switching users**: `UserApiHelper.loginToUser()` just signs the new user in — there is one cookie, so that replaces the previous session, and a failed switch still surfaces because `login()` asserts the response status. Restore the admin with `ApiHelpers.loginToAdminUser()` when the test is done.
- **Reusing the session**: `tests/auth.setup.ts` signs in **over the API** once and writes `storageState`; every authenticated project depends on that `setup` project and loads the state. Add a new project the same way rather than signing in per spec. Don't reintroduce a UI login there — it costs every project a back-office boot, and the login screen is covered by `DefaultConfig/Login/BackOfficeLogin.spec.ts`.

`ApiHelpers.updateTokenAndCookie()` and `ApiHelpers.revokeTokens()` survive as deprecated shims over `signIn`/`signOut` — do not use them in new code.

---

## 6. Generated / ignored files

- `console-errors.json` is generated at install/run time and is **git-ignored** — do not commit it. Console errors captured during a run are appended here for inspection; they are not (yet) a failing gate.
- `.env`, `playwright/.auth/`, and `results/` are also ignored.
