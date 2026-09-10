---
name: umb-e2e-test
description: Write, extend, or repair Playwright acceptance (end-to-end) tests for Umbraco CMS in tests/Umbraco.Tests.AcceptanceTest. Use whenever the work involves a .spec.ts under that project, one of its UiHelper/ApiHelper page objects, or a flaky/failing E2E test — including phrasings like "add an e2e test", "add an acceptance test", "this Playwright test is flaky", "the E2E test fails intermittently", "add a UI helper", "test this in the backoffice end-to-end", or "why is this spec never running". Also use before hand-writing a new spec file, so it lands in a directory Playwright actually runs. Does NOT apply to C# unit or integration tests (tests/Umbraco.Tests.UnitTests, tests/Umbraco.Tests.Integration) or to backoffice Vitest tests under src/Umbraco.Web.UI.Client.
---

# Umbraco Acceptance Tests (Playwright)

Procedure for writing and repairing the Playwright E2E suite in `tests/Umbraco.Tests.AcceptanceTest`.

`CLAUDE.md` in that project is the reference for *why* each rule exists; this skill is the order to do things in. Read the **12-row checklist at the top of its §3** once per session before editing a spec or helper, and drop into a subsection when you hit that specific case — the section itself is long because the reasons are, not because the rules are. `npm run audit` catches the mechanical half.

**All paths below are relative to `tests/Umbraco.Tests.AcceptanceTest`.** Prefer paths over `cd`.

## Key facts

- **A spec only runs if it sits in a directory a `playwright.config.ts` project matches** — `DefaultConfig/**`, `ExtensionRegistry/**/*.spec.ts`, `EntityDataPicker/**/*.spec.ts`, `DeliveryApi/**`, `ContentSettingConfig/**`, `SMTP/*.spec.ts`, `ImagingSettingConfig/*.spec.ts`, `ExternalLogin/AzureADB2C/**`, `AuthProviderLateRegistration/**/*.spec.ts`, `UnattendedInstallConfig/**`. A file written straight into `tests/` type-checks, looks correct, and is **silently never run**. Default to `tests/DefaultConfig/`.
- **Two gates run without an Umbraco instance**: `npm run typecheck` (covers `lib/` *and* `tests/`; `npm run build` compiles `lib/` only and misses spec type errors) and `npm run audit` (the conventions in `CLAUDE.md` §3). Run both before committing.
- **There is no lint step**, but `npm run audit` covers the mechanical part: it fails at budget 0 on dropped promises, silently-discarded assertions, specs no project runs, raw `page` use in a spec, hardcoded endpoints, and un-annotated skipped tests. What debt is left (sleeps, force clicks, hardcoded indexes, raw response assertions) is budgeted so it can shrink but not grow — see `CLAUDE.md` §7 for which rules are gates and which are ratchets. **All of these run in CI** (`build/azure-pipelines.yml`, `Build` stage, job C), so a regression fails the build.
- **Running the suite needs a running, installed Umbraco** on `https://localhost:44339` using SQL Server/LocalDB (SQLite is too slow for the suite), plus a `.env` with superadmin credentials. Without one you can write and type-check, but you cannot verify.
- **`workers: 1`, `retries: 2`, 60s test timeout, 5s expect timeout.** Specs share fixed entity names, so they must not run in parallel.
- In-repo specs import from `@umbraco/acceptance-test-helpers` — a `tsconfig.json` path alias onto `lib/index.ts`, **not** the published package name (`@umbraco-cms/acceptance-test-helpers`). Use the alias in-repo.

## Procedure

### 1. Decide what kind of change this is

- **New scenario, existing page objects cover it** → new spec only (step 2).
- **New scenario needing a locator or action that does not exist** → helper first (step 3), then the spec.
- **Existing spec is flaky or failing** → skip to step 6.

### 2. Create the spec

Use the generator rather than hand-writing the file — it puts the spec in a directory that runs and scaffolds the conventions:

```bash
npm run createTest MyFeatureName              # -> tests/DefaultConfig/MyFeatureName.spec.ts
npm run createTest MyFeatureName DeliveryApi  # -> tests/DeliveryApi/MyFeatureName.spec.ts
```

It refuses to overwrite an existing file. Then shape the body:

- **Set up via API, assert via UI.** Creating fixtures through `umbracoApi.*` is far faster and less brittle than clicking them into existence. Drive the actual user workflow through `umbracoUi.*`, then confirm the result through the UI state and an API read where that is cheap.
- **Idempotent cleanup in both hooks.** `ensureNameNotExists()` in `beforeEach` *and* `afterEach` — never a bare `delete()`, which throws when a prior run already removed the entity.
- **Names unique to the file.** The suite is serial and shares names like `TestContent`; a name reused across files means one spec's cleanup can delete another's fixture.
- **Tag it** `{ tag: '@smoke' }` for a critical path, `{ tag: '@release' }` for full release validation. Untagged specs still run in the main suite.

### 3. Put a new helper in the right layer

```
BasePage            lib/helpers/BasePage.ts        low-level wrappers (click, enterText, isVisible, waitForVisible, ...)
  └─ UiBaseLocators lib/helpers/UiBaseLocators.ts  shared locators, composed actions, the response-waiting helpers
       └─ <X>UiHelper  lib/helpers/<X>UiHelper.ts  page object for one area (Content, Library, Media, ...)

ApiHelpers          lib/helpers/ApiHelpers.ts      HTTP transport against the Management API
  └─ <X>ApiHelper   lib/helpers/<X>ApiHelper.ts    per-entity setup/teardown
```

Put it in the **most specific layer that covers the need**: an area-specific action belongs in that area's `<X>UiHelper`, and only something genuinely shared across areas moves up to `UiBaseLocators`. Reuse the shared primitives (`this.click`, `this.isVisible`, `this.getTextLocatorWithName`) rather than reaching for `this.page` directly.

`lib/` ships as an npm package, so exported signatures are a **public contract** — add an optional parameter or an overload rather than changing or removing an existing one. Put new endpoint paths, status codes and message strings in `ConstantHelper` instead of inlining them.

**For a builder**: declare the payload shape in `lib/builders/types.ts`, give `build()`/`getValues()` that return type, and make sure no `: any` sits on the path to it — an `any` in a subclass swallows the mistake before the return type ever sees it, which makes the annotation decorative. See `CLAUDE.md` §5.

### 4. Apply the determinism checklist

Walk this before committing. Each item is a real failure mode in this suite:

1. **Every async call awaited.** Assertions and helper calls are all async. Never `array.forEach(async ...)` — the callback's promise is dropped and the assertion never runs, so the test greens even when it fails. Use `for (const x of xs)`.
2. **No new fixed sleeps.** `page.waitForTimeout` / `ConstantHelper.wait.*` are the largest flakiness source. The suite still holds 91 (`ContentUiHelper`, `LibraryUiHelper` and `UiBaseLocators` worst), only 10 of them justified — budgeted debt, not precedent. The audit counts **unjustified** ones, so if you genuinely need a sleep, the comment saying what it stands in for is what keeps the build green. Wait on state instead:
   - a getter → `await expect.poll(() => api.isDocumentPublished(id), {timeout: ConstantHelper.timeout.veryLong}).toBeTruthy();`
   - a flow that can lose an early click → wrap it in `expect(async () => { ... }).toPass({timeout: ConstantHelper.timeout.medium})`
   - an API round-trip → `waitForResponseAfterExecutingPromise(url, promise, statusCode, method?)`, never a sleep after the click
   - a create → `waitForWorkspaceEditRoute('document' | 'element')`, because a create is not finished until the workspace re-routes to `.../edit/{id}`

   If there is genuinely no observable state to wait on, leave a comment saying what the sleep stands in for.
3. **Response waits disambiguated.** URL matching is substring, so `apiEndpoints.document` also matches `/document-type`, `/document-blueprint` and `/document/{id}/publish`. With several calls in flight, pass `method` and/or a longer `url` fragment. For several matching responses from one action use `waitForMultipleResponsesAfterExecutingPromise`; for one 201 per endpoint use `waitForCreatedResponsesAfterExecutingPromise`, which matches with `endsWith` so prefixes cannot collide.
4. **`this.click()`, not raw `.click()`.** The wrapper waits for visibility first. Use a raw `.click()` only where skipping that wait is deliberate.
5. **Entity names matched exactly.** Locators are strict-mode: two matches throw. Leftover data from a crashed run (`TestUserGroupNameDescription` when you meant `TestUserGroupName`) is what makes a substring locator fail. For `uui-table-row` use `.filter({has: this.getTextLocatorWithName(name)})`; for `umb-entity-item-ref` filter on the rendered `[name="..."]` attribute. `{hasText: ...}` stays correct for *structural* filtering by a fixed label ("Document permissions") — the rule is about entity names.
6. **A new check asserts internally, and every call site awaits it.** 451 of 464 `does*`/`is*`/`has*` methods `await` an assertion and return void. If your method returns a value instead, name it `get*`/`count*`/`find*` so the call site reads as needing an assertion. **Never drop the `await`**: these are `async` with synchronous assertions, so an unawaited call still runs them but turns a failure into an unhandled rejection — Playwright then prints `1 passed` plus "1 error was not a part of any test". The audit fails at budget 0 on both halves (`discardedCheck`, `unawaitedAssertion`).
7. **`force: true` gets a comment.** It disables the visibility/stability/hit-target checks, which is how a masked UI bug becomes a pass. Say which check you are overriding and why that is safe — the audit counts force clicks that lack one.
8. **A disabled test gets an annotation, not a comment**: `test.skip('...', {annotation: {type: 'issue', description: '<what blocks it, with the GitHub link>'}}, async ({...}) => {`. Annotations surface in reports; comments don't. Prefer `type: 'issue'` with a link over `type: 'blocked'` with prose. If the feature was removed, delete the test rather than skipping it forever.
9. **Assert through a helper, not on the raw API response.** `expect(contentData.variants[0].state).toBe('Published')` couples the spec to the response shape and fails without naming the entity. Thirteen helpers cover the common shapes — see the table in `CLAUDE.md` §3. They match by **alias**, not by the position `values[0]` wrongly assumes. For a nested shape use `getPropertyValue(data, alias)`, or `getOnlyPropertyValue(data)` when no alias is in play (it asserts there is exactly one property, so the assumption is stated rather than buried in an index). Add a sibling helper for shapes they don't cover; don't add raw assertions (the audit fails).
10. **Never comment out a test or an assertion.** A commented test is invisible to `--list` and every reporter; a commented assertion leaves a passing test checking less than it looks like. And anchor every TODO — `// TODO (V19): …` or `// TODO: … [XX]` — or it never gets removed.
11. **JSDoc the surprising, not the self-evident.** `clickSaveButton()` needs none. A helper whose contract isn't obvious from its name does. Coverage is 3% by design — don't sweep it.

### 5. Verify

```bash
npm run check   # typecheck + audit (self-tests its rules first) + API helper tests
```

If you add an audit rule, add its `bad` **and** `good` fixture cases to `audit-selftest.js` in the same commit — the self-test fails on any rule with no case.

If you change what an API assertion helper *means*, add a case to `helpers-selftest.js` first. They back ~700 spec assertions, so a semantic change silently changes what all of those assert — and the shape of mistake to watch for is a check that counts `values` entries instead of distinct aliases, which passes on invariant content and fails on every variant and segment spec. See `CLAUDE.md` §3.

The same file also pins **builder payloads**. When you add a `withX()`, check the field is actually read in `build()`/`getValues()`/`getValue()` and add a case asserting the built payload carries it — a setter that stores a field nothing reads is a silent no-op that no test run, type-check or audit will catch. See `CLAUDE.md` §5.

If you reduced a budgeted count (removed a sleep, tightened a locator), **lower that budget in `audit-conventions.js` in the same commit** — the audit prints the new number. Never raise a budget to make a run pass.

You can also confirm Playwright still collects what you expect without a running site:

```bash
npx playwright test --list | tail -1   # e.g. "Total: 1651 tests in 270 files" (270 counts auth.setup.ts)
```

Then, if an instance is available, run the narrowest thing that covers the change, more than once — a single green does not distinguish a deterministic test from a lucky one:

```bash
npx playwright test tests/DefaultConfig/MyFeatureName.spec.ts
npx playwright test -g "can create content"                                 # single test by name
npx playwright test --headed tests/DefaultConfig/MyFeatureName.spec.ts
npx playwright test --repeat-each 3 tests/DefaultConfig/MyFeatureName.spec.ts  # flakiness check
```

A run writes `results/results.json`, and `npm run flaky` reads it — a test that failed twice and passed on the third attempt is reported green, so a green run is not by itself evidence the spec is deterministic. Check it before calling the change verified. It also names any test whose slowest attempt ran past half the 60s timeout, which is the state a spec is in shortly before it becomes flaky. See `CLAUDE.md` §3.

If you cannot run it, say so plainly and name the specs that need running. Do not describe an unrun change as verified.

### 6. Repairing a flaky or failing spec

Find the cause before changing anything; the checklist in step 4 is the list of usual suspects, in rough order of likelihood.

1. **Read the trace** rather than guessing — failures keep one (`trace: 'retain-on-failure'`): `npx playwright show-trace results/trace.zip`.
2. **Reproduce it.** `--repeat-each 5` on the single spec. Passing 5/5 points at cross-spec leftover state rather than the spec itself.
3. **Check for leftover state.** A strict-mode multi-match, or a name shared with another spec, points at cleanup. Known cleanup gaps: some `ensureNameNotExists` / `recurseChildren` paths delete only the **first** match, and list fetches use one large `take` with no pagination, so duplicates and very large trees can leave residue.
4. **Fix the wait, not the symptom.** Lengthening a sleep or a timeout buys time; it does not make the test deterministic. Replace the sleep with a wait on the state it was standing in for.
5. **Confirm the fix.** `--repeat-each` again — a flaky test that passes once after an edit proves nothing.

## Gotchas

- `console-errors.json` is generated and git-ignored; never commit it. Console errors are collected for inspection and are not yet a failing gate.
- `.env`, `playwright/.auth/`, `results/` and `dist/` are ignored too.
- In UI mode showing only the authenticate test: open 'Projects' and select `defaultConfig`.
- Authenticated projects depend on the `setup` project for stored `storageState`. `unattendedInstallConfig`, `externalLoginAzureADB2C` and `authProviderLateRegistration` run unauthenticated on purpose.
- `forbidOnly` is on for CI — a stray `test.only` fails the build.
- Some scripts exist because of environment limits, not preference: `testSqlite` (drops User tests) and `testWindows` (drops RelationType tests).
