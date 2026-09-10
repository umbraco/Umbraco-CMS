# Umbraco Acceptance Tests — Contributor & Agent Guide

Playwright end-to-end tests for Umbraco CMS. This file covers **how the suite is built and the conventions that keep it deterministic**. For setup, running, and writing your first test, see [README.md](./README.md) first — this document does not repeat it.

---

## 1. Gates you can run without an Umbraco instance

```bash
npm run check       # all three of the below, in order

npm run typecheck        # tsc -p tsconfig.json --noEmit  — lib/ AND tests/
npm run audit            # conventions from §3 (self-tests its own rules first), see §7
npm run helpers:selftest # the API assertion helpers, against fabricated responses
```

Run `npm run check` before committing. None of it needs a running site, so there is no excuse to skip it.

`npm run build` compiles `lib/` only (`tsconfig.build.json`) and does **not** type-check the specs under `tests/` — that is what `npm run typecheck` is for.

There is no lint step. §7 covers what `npm run audit` can check mechanically; everything else in §3 you apply by hand.

A spec also has to sit **inside a directory that one of the `playwright.config.ts` projects matches** (`DefaultConfig/**`, `DeliveryApi/**`, `SMTP/*.spec.ts`, …). A file written straight into `tests/` type-checks, looks fine, and is silently never run — no project's `testMatch` claims it. `npm run createTest` writes into `tests/DefaultConfig/` for exactly this reason.

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
- `lib/` is published to npm as **`@umbraco-cms/acceptance-test-helpers`** (see `package.json`). Treat exported helper signatures as a **public contract** — prefer adding optional parameters or new overloads over changing/removing existing ones.
- Specs in this repo import from **`@umbraco/acceptance-test-helpers`**, which is *not* the published name — it is a `tsconfig.json` path alias onto `lib/index.ts`. Both spellings are live and neither is wrong: use the alias in-repo, and the published name in anything a consumer reads.

### Deprecating an exported helper

The root `CLAUDE.md` §6 spells out an exacting policy for C# — `[Obsolete("… Scheduled for removal in Umbraco {current+2}.")]`, a minimum of one full major as a deprecation period. **That policy covers C# only**, and `lib/` is a published, semver-versioned npm package with `peerDependencies` and external consumers. So apply the same shape here:

```ts
/**
 * @deprecated Waits on a plain update, not the publish response.
 * Prefer {@link clickSaveAndPublishButtonAndWaitForContentToBePublished}.
 * Scheduled for removal in 20.0.
 */
```

Three parts, all of them load-bearing: **why** it is wrong, **what to use instead** (as a `{@link}`, so an IDE takes you there), and **when it goes**. The five existing `@deprecated` markers in `lib/` all carry the first two and none carries the third — so nothing tells a consumer how long they have, and nothing tells us when it is safe to delete. Add the removal version to any deprecation you touch.

Never change or remove an exported signature in a minor. Add an optional parameter or a new overload, deprecate the old one, and let it ride to the scheduled major.

---

## 3. Flakiness-avoidance conventions

These are the rules that keep the suite stable. Most flaky failures trace back to breaking one of them.

**This section is long because the *reasons* are long. The rules are not.** Read the checklist; drop into a subsection when you hit that case.

| | Rule | Detail |
|-|------|--------|
| 1 | `await` every assertion and helper call; never `forEach(async …)` | [Never drop a promise](#never-drop-a-promise) |
| 2 | Wait on state, not on time — no new fixed sleeps without a comment | [Wait on state, not on time](#wait-on-state-not-on-time) |
| 3 | Use `waitForResponseAfterExecutingPromise`, disambiguated | [Waiting for API responses](#waiting-for-api-responses) |
| 4 | `this.click()`, not a raw `.click()` | [this.click() vs raw .click()](#thisclick-vs-raw-click) |
| 5 | `force: true` needs a comment saying which check it overrides | [force: true](#force-true-needs-the-same-justification-as-a-sleep) |
| 6 | A `does*`/`is*`/`has*`/`verify*` check asserts internally — and every call site awaits it | [A check either asserts or returns](#a-check-either-asserts-or-returns--never-both-shapes-under-one-name) |
| 7 | Assert through a helper, never on a raw API response shape | [Assert through a helper](#assert-through-a-helper-not-on-the-raw-api-response) |
| 8 | One property is not one `values` entry — variants and segments add entries | [One property is not one values entry](#one-property-is-not-one-values-entry) |
| 9 | Ask what would have to break for an assertion to fail | [An assertion that cannot distinguish pass from fail](#an-assertion-that-cannot-distinguish-pass-from-fail) |
| 10 | `.nth(N)` only where the index is the subject | [Index-based locators](#index-based-locators-only-where-the-index-is-the-subject) |
| 11 | Match entity names exactly, so leftover data can't multi-match | [Match names exactly](#match-names-exactly-to-survive-leftover-data) |
| 12 | Read the component before choosing a locator form | [Verify a locator against the component](#verify-a-locator-against-the-component-not-against-a-hunch) |

`npm run audit` enforces the mechanical half of every one of these — see §7 for which, and at what budget.

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

**Before blaming the test, check the instance.** Scheduled publishing firing mid-test is a flake the suite guards against *in CI only* — the nightly pipeline copies `SuspendScheduledPublishingComposer` (and SQL Server delayed durability) from `tests/Umbraco.Tests.AcceptanceTest.UmbracoProject/` into the instance it builds. A local instance has neither, and `src/Umbraco.Web.UI` additionally runs ModelsBuilder in `InMemoryAuto`, regenerating models on every content-type change in a suite that changes them constantly. A flake that reproduces only locally is often that difference. See README.md → Prerequisites.

**Known debt — the suite has not finished paying this off.** 91 `waitForTimeout` calls remain, concentrated in `ContentUiHelper`, `LibraryUiHelper` and `UiBaseLocators`. Only **10 of them carry a justification**; the other 81 are what `npm run audit` budgets. Treat them as debt, not as precedent: the fact that a neighbouring method sleeps is not a reason for a new one to.

`BasePage.waitForTimeout()` and `UiHelpers.waitForTimeout()` exist and are exported, so they stay — but reach for them only when there is genuinely no observable state to wait on, and **leave a comment saying what the sleep stands in for** (as `FormsUiHelper` does). That comment is the contract: the audit counts unjustified sleeps only, so a justified one is allowed and does not fail the build. When you remove one, replace it with a wait on the state it was covering for, and verify the spec still passes against a running instance.

### Waiting for API responses
`UiBaseLocators.waitForResponseAfterExecutingPromise(url, promise, statusCode, method?)` resolves on the first response whose URL **contains** `url`, whose status matches, and — when `method` is given — whose request method matches. It **returns the affected id**: the `Location` header's last segment for a 201, otherwise the trailing path segment of the response URL. Endpoint constants live in `ConstantHelper.apiEndpoints`, status codes in `ConstantHelper.statusCodes`, methods in `ConstantHelper.httpMethods`.

- The match is a substring, so endpoint constants are prefixes of each other: `apiEndpoints.document` also matches `/document-type`, `/document-blueprint`, `/document/{id}/publish`. When more than one such call is in flight, pass `method` and/or a more specific `url` fragment to disambiguate.
- Two siblings cover the multi-response cases:
  - `waitForMultipleResponsesAfterExecutingPromise(url, promise, statusCode, expectedCount)` — one action that fires several matching calls (e.g. moving a selection). Same loose `includes` matching; returns the ids in arrival order.
  - `waitForCreatedResponsesAfterExecutingPromise(urlEndings, promise)` — one 201 per entry in `urlEndings`, matched with `endsWith` rather than `includes`, so prefix-overlapping endpoints don't collide. Returns the created ids in the order of `urlEndings`.
- `waitForWorkspaceEditRoute('document' | 'element')` waits for the workspace to re-route from `.../create/...` to `.../edit/{id}`. A create isn't finished until that reload lands — a variant/segment switch started beforehand is discarded and its edits lost, so the update response you were awaiting never fires.
- **"Save and publish" hits different endpoints for new vs existing content**: a new document publishes via `POST /document/create-and-publish` (201), an existing one via `PUT /document/{id}/update-and-publish` (200), and the workspace then re-fetches `GET /document/{id}/published`. Waiting on the generic `apiEndpoints.document` (status 200) resolves on whichever 200 `/document` response follows the click, so the same helper works for both cases — don't narrow it to a single publish sub-endpoint.

### `this.click()` vs raw `.click()`
`BasePage.click()` waits for the element to be visible before clicking. Use it. Reach for a raw locator `.click()` only when you deliberately need to skip that wait.

### `force: true` needs the same justification as a sleep
`force: true` switches off Playwright's actionability checks — visibility, stability, hit-target. That is exactly how "the element is covered", "the element is still animating" and "another element is intercepting the click" stop being failures and become passes. It is a legitimate escape hatch (a known-harmless overlay, a control the browser reports as unstable but is fine to hit), but it is never free.

The suite has **79 force clicks, none of them justified** — same debt shape as the sleeps, and budgeted the same way. Write a comment saying which actionability check you are overriding and why it is safe; a force click with no comment is indistinguishable from a masked bug, and the audit counts it.

### A check either asserts or returns — never both shapes under one name
`does*` / `is*` / `has*` methods come in two kinds, and **451 of the 464 in `lib/` assert internally** (they `await` an assertion and return void):

```ts
// asserts internally -> a bare await is the correct call
async isContentInTreeVisible(name: string) {
  await this.isVisible(this.treeItem.filter({has: this.getTextLocatorWithName(name)}));
}

// returns a value -> the CALLER must assert, or the check silently does nothing
async doesNameExist(name: string) {
  return await this.getByName(name);
}
```

The two are indistinguishable at the call site, and they fail in opposite directions. Bare-`await` a value-returning check and **the assertion evaporates — the test greens no matter what the UI did.** That is the dropped-promise failure mode wearing a different hat, and it leaves no trace in a report.

So: **a new `does*`/`is*`/`has*` method asserts internally.** If it has to hand a value back, give it a `get*` / `count*` / `find*` name instead, so the call site reads as something that obviously needs an assertion.

**And always `await` it.** The failure mode is nastier than it looks. These helpers are `async` but their assertions are synchronous, so a call without `await` still *runs* every assertion — and a failure becomes a rejected promise nobody handles. Playwright then reports:

```
1 passed (1.2s)
1 error was not a part of any test, see above for details
```

A real assertion failure reads as a **green test**, with the error demoted to a line a CI summary will skip. Verified by probe, not inferred.

`npm run audit` fails at budget 0 on both halves of this: a value-returning check that nothing asserts on (`discardedCheck`), and an assert-internally helper called without `await` (`unawaitedAssertion`). Both are currently 0 — every one of the ~730 migrated call sites is awaited, and the 12 value-returning checks are all wrapped in `expect()`.

### Assert through a helper, not on the raw API response

A spec that reaches into a response shape is doing the same thing as one that reaches for `page.locator` — it just does it on the API side, where nothing flags it:

```ts
// ✗ hard-codes the response shape, assumes an order the API never promised,
//   and fails with "expected 'Published', received 'Draft'" - which document?
expect(contentData.variants[0].state).toBe('Published');
expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
expect(contentData.values[0].value).toEqual(text);

// ✓
await umbracoApi.document.doesVariantHaveState(contentData, 'Published');
await umbracoApi.document.doesPropertyHaveValue(contentData, AliasHelper.toAlias(dataTypeName), text);
```

The full set lives on `ApiHelpers`, delegated onto the helper each entity is fetched from:

| Helper | Asserts | On |
|--------|---------|-----|
| `doesVariantHaveState(data, state, culture?)` | publication state | `document`, `element` |
| `doesVariantHaveName(data, name, culture?)` | variant name | `document`, `element` |
| `doesHaveVariantCount(data, count)` | how many variants | `document`, `element` |
| `doesPropertyHaveValue(data, alias, value, culture?)` | one property value, matched by alias | `document`, `element` |
| `doesHaveValueCount(data, count)` | how many `values` are set; `0` means none | `document`, `element`, `dataType` |
| `doesDataTypeHaveEditors(data, alias, uiAlias)` | both editor aliases together | `dataType` |
| `doesHaveContent(data, content)` | file content | `template`, `partialView`, `stylesheet`, `script` |
| `doesHavePropertyCount(data, count)` | how many property *definitions* | `documentType`, `mediaType`, `memberType` |
| `doesHaveCompositionCount(data, count)` | how many compositions | `documentType`, `mediaType`, `memberType` |
| `doesPropertyUseDataType(data, alias, id)` | which data type backs a property | `documentType`, `mediaType`, `memberType` |
| `doesOnlyPropertyUseDataType(data, id)` | exactly one property, backed by that data type | `documentType`, `mediaType`, `memberType` |
| `isElementType(data, expected?)` | element-type flag | `documentType` |
| `getPropertyValue(data, alias, culture?)` | **returns** a value for a nested assertion | `document`, `element`, `dataType` |
| `getOnlyPropertyValue(data)` | **returns** the sole value, asserting there is one | `document`, `element`, `dataType` |

Note the deliberate split between `values` and `properties`: a content *item* carries property **values** (`doesHaveValueCount`), a content *type* carries property **definitions** (`doesHavePropertyCount`). Same word, different arrays — the helpers live on different objects so they cannot be confused at a call site.

#### One property is not one `values` entry

**The trap these helpers exist to survive.** A property that varies by culture or segment contributes **one `values` entry per culture/segment**, so a single-property document legitimately carries several:

```jsonc
// one property, two cultures
"values": [
  {"alias": "title", "culture": "en-US", "value": "hello"},
  {"alias": "title", "culture": "da",    "value": "hej"}
]
```

So an "is there exactly one property?" check must count **distinct aliases**, never `values.length`. The first version of `getOnlyPropertyValue` counted entries, and would have failed every variant and segment spec it was written to serve — while passing the invariant ones, which is what makes this shape of mistake easy to ship.

The resulting contract:

- `getOnlyPropertyValue(data)` — asserts exactly one distinct alias, returns the **first** entry (what the raw `values[0].value` returned).
- `getPropertyValue(data, alias)` / `doesPropertyHaveValue(data, alias, value)` — with no culture, several matches is legitimate and the first is used; pass a culture when a specific variant is the subject, and then exactly one match is required.
- `doesVariantHaveState` / `doesVariantHaveName` — with no culture, target `variants[0]`, the default variant.

Every one of those cases is pinned by `npm run helpers:selftest`, which runs the helpers against fabricated response objects and needs no Umbraco instance. **Add a case there before changing any of these semantics** — these helpers back ~700 spec assertions, so a change to what they mean silently changes what all those specs assert.

`getPropertyValue` is deliberately named `get*`, not `does*`: it hands a value back, so the caller must assert on it (§3's check-method rule). It exists for the nested shapes a generic assertion cannot cover, and it keeps the nested assertion — which is the actual subject of such a test — while dropping the positional lookup:

```ts
// ✗ two assertions, the first only to prove the second is looking at the right property
expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
expect(contentData.values[0].value.src).toContain(imageName);

// ✓ the alias becomes the lookup key
expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName)).src).toContain(imageName);
```

Note `doesDataTypeHaveEditors` takes both aliases: a data type with the right `editorAlias` but the wrong `editorUiAlias` renders the wrong editor in the backoffice, so asserting one without the other passes on a genuinely broken data type.

Three things they buy:

1. **The shape lives in one place.** `variants[0].state` appeared in 45 spec files; a response change meant 45 edits.
2. **Failures name the entity.** `Expected the default variant of 'TestContent' to be Published` beats `Published != Draft`.
3. **`doesPropertyHaveValue` matches by alias, not by position** — `values[0]` assumed an ordering the API does not guarantee, so it could pass or fail on seed order alone. This is the same trap as a hardcoded `.nth(0)` (below), on the API side.

They take the **already-fetched entity** rather than a name on purpose: `getByName` walks the tree recursively, so a name-based overload would re-request on every assertion.

**211 raw-response assertions remain**, down from 890 (-76%), and the count is budgeted.

The two `get*` helpers are what made the deep cases tractable. `getOnlyPropertyValue` in particular replaces `values[0].value` where no alias is in play: it asserts there *is* exactly one property and returns it, so the single-property expectation is stated instead of buried in an index — and a spec that later grows a second property fails loudly rather than silently asserting against whichever sorts first. Both stop at the entity boundary: `getOnlyPropertyValue(data).contentData[0].values[0].value` still indexes into the *block's own* structure, which is that test's actual subject.

What is left is a genuine long tail: no shape appears more than nine times, spread across roughly twenty-five distinct ones (user-group permission flags, domain routes, block colours, pagination totals). Writing twenty-five helpers for five call sites each would inflate a published package's public surface for very little, so these stay. Add a helper when you touch one and it earns its place; do not add new raw assertions, since the audit will fail.

### An assertion that cannot distinguish pass from fail

The audit is a text scanner; no rule in it can tell whether a test asserts the *right* thing. Reading specs is the only way to close that gap, and this is the shape to look for:

```ts
// ✗ the filename is `{id}.udt` with or without descendants, so this passes identically
//   whether or not the descendants option does anything
const exportData = await umbracoUi.dictionary.exportDictionary(true);
expect(exportData).toEqual(parentDictionaryId + '.udt');

// ✓ only the contents can show the descendant was included
const exportData = await umbracoUi.dictionary.exportDictionaryAndReadFile(true);
expect(exportData.filename).toEqual(parentDictionaryId + '.udt');
expect(exportData.content).toContain(`Name="${parentDictionaryName}"`);
expect(exportData.content).toContain(`Name="${dictionaryName}"`);
```

Both dictionary export tests had that shape, and the "with descendants" one is `@smoke` — in the fast-feedback set while asserting nothing about descendants.

**Be most suspicious of a pair of tests whose only difference is a flag.** If both assert the same thing, one of them is not testing its own name. The general question to ask of any assertion: *what would have to break for this to fail?* If the answer is "the download event" in a test named for export contents, the assertion is in the wrong place.

That shape is mechanically findable, which is worth knowing: normalise test names by removing flag words (`with`/`without`, `can`/`cannot`, `enabled`/`disabled`), group tests that collapse to the same key, and compare their assertion lines. Across all 269 specs that produced exactly two candidates, and both were real.

#### When the setup cannot exercise the subject

The worse version is a test no assertion can save. `ContentWithContentPicker.spec.ts` has a pair differing only in whether the data type sets **ignore user start nodes** — and that setting only changes which nodes a user *whose account is start-node-restricted* may pick. Both tests run as the shared admin, who has no restriction, so the flag has no observable effect in either, and no stronger assertion can create one.

Both now at least assert the pick persisted rather than only that a success notification appeared, and the limitation is recorded at the setup line. Verifying the setting itself needs a restricted user — `tests/DefaultConfig/Users/Permissions/User/ContentStartNodes.spec.ts` is the pattern.

So when a test's subject is a permission, a restriction, or a role-scoped setting, check **who the test runs as** before trusting it. The shared admin bypasses most of them, which makes such a test quietly vacuous rather than failing.

### Index-based locators only where the index is the subject
`.nth(i)` is right for "the i-th block" when the index is a parameter and the position is what the test is about — 110 of the suite's 124 uses are that. A **hardcoded** `.nth(0)` / `.nth(2)` is different: it bakes in an assumption about list order that neither the backend nor the seed data promises, and produces exactly the coincidental pass the root `CLAUDE.md` §10 warns about. There are 15, mostly in `FormsUiHelper` and `DataTypeUiHelper`. Prefer a locator that names what it wants (a label, a `[name=...]`, an exact text); reach for a literal index only when nothing distinguishes the elements.

### Match names exactly to survive leftover data
Playwright locators run in **strict mode** — if a locator resolves to more than one element, the action throws. Residue from a crashed/partial run (e.g. a leftover `TestUserGroupNameDescription`) makes a substring locator for `TestUserGroupName` match two rows and fail. Match on exact text whenever the value is an entity name:

```ts
// ✗ also matches 'TestUserGroupNameDescription' → strict-mode multi-match
this.page.locator('umb-user-group-ref', {hasText: name});

// ✓ exact
this.page.locator('umb-user-group-ref').filter({has: this.page.getByText(name, {exact: true})});
```

Two forms are established in `lib/`; pick by what the component renders:

| Element | Use | Because |
|---------|-----|---------|
| `uui-table-row` (collection/list-view rows) | `.filter({has: this.getTextLocatorWithName(name)})` | the name is its own text node — `getTextLocatorWithName` is the shared `getByText(name, {exact: true})` on `UiBaseLocators` |
| `umb-entity-item-ref` (pickers, workspace refs) | ``.filter({has: this.page.locator(`[name="${name}"]`)})`` | the ref renders the name as an attribute, so this is an exact match and cheaper than a text scan |

This is why `UserGroupUiHelper`/`UserUiHelper` match exactly rather than with `hasText`.

#### Endpoint constants are checked against the API's own contract

`ConstantHelper.apiEndpoints` hardcodes 46 Management API paths. Nothing used to tie them to the API, so a renamed route surfaced as a helper waiting for a response that never arrives — a 60-second timeout with no hint of the cause. `npm run audit` now cross-checks every constant against the committed `src/Umbraco.Cms.Api.Management/OpenApi.json` (428 paths), which needs no running instance.

The check covers **inline paths too, not just the constants** — and it has to: only 31 call sites use `ConstantHelper.apiEndpoints`, while **232 write the path inline** in an `*ApiHelper`. Checking constants alone would have verified about a eighth of the suite's API surface. (Migrating those 232 to constants is separate, optional tidying; what matters for correctness is that the route exists.)

Three auth paths are allow-listed — `security/back-office/revoke`, `/token` and `/login` — because OpenIddict middleware serves them rather than an MVC controller, so they never appear in a generated document. If you add an auth-adjacent endpoint, expect to allow-list it. **Anything else failing this rule means the route moved.**

Turning it on found seven paths that had already moved:

| Helper | Was | Now |
|--------|-----|-----|
| `DataTypeApiHelper.getItems` | `/tree/data-type/item` | `/item/data-type` |
| `DictionaryApiHelper.getItems` | `/tree/dictionary/item` | `/item/dictionary` |
| `RelationTypeApiHelper.getItems` | `/relation-type/item` | `/item/relation-type` |
| `ScriptApiHelper.getItems` | `/tree/script/item` | `/item/script` |
| `TemplateApiHelper.getItems` | `/tree/template/item` | `/item/template` |
| `RelationTypeApiHelper.getAllAtRoot` | `/tree/relation-type/root` | `/relation-type` (no tree endpoint exists) |
| `PublishedCacheApiHelper.getStatus` | `/published-cache/status` | `/published-cache/rebuild/status` |
| `UserApiHelper.updateCurrentUserPassword` | `/user/change-password/` | `/user/current/change-password` |

Every one of those methods is uncalled by any spec or helper, so **no test outcome changed** — they were dead published API that would 404 for a consumer. That is also why the fix was safe to make without a running instance: nothing could break. Each replacement was taken from `OpenApi.json`, and for the relation-type one the paged list was confirmed to accept the same `skip`/`take` and return the same `{total, items}` shape `itemsOf` expects.

This check is also the practical coverage for thin API wrappers like `ObjectTypesApiHelper` and `PublishedCacheApiHelper`, which no spec exercises: for a two-line method whose whole job is a URL, "does the route exist" is most of the contract.

### Verify a locator against the component, not against a hunch

The backoffice source is in this repo, so you can check what an element actually renders **without a running instance** — and you have to, because the rendering differs per entity in ways that decide which locator form works:

| Element | Renders the name as | Locator that works |
|---------|--------------------|--------------------|
| `umb-entity-item-ref` (default, e.g. stylesheet) | `<uui-ref-node name=${item.name}>` | `[name="..."]` |
| `umb-entity-item-ref` for language / user | custom refs, both also `name=${item.name}` | `[name="..."]` |
| user-group / webhook collection rows | `<a href=...>${name}</a>` — light-DOM text | `getByText(name, {exact: true})` |
| document / element collection rows | `<uui-button label=${name}>` — an **attribute** | `[label="..."]` |

That last row is the trap: a collection row's name is a `label` attribute whose only text lives inside `uui-button`'s shadow DOM, so the `getByText(exact)` form proven on user-group and webhook rows does **not** transfer to document and element rows. Matching the attribute is exact, and independent of how `uui-button` chooses to render.

So before converting a locator: find the element under `src/Umbraco.Web.UI.Client/src/packages/`, read its `render()`, and pick the form that matches what it emits. A pre-existing passing usage of the same form on the same element counts as verification too. Where neither is available, leave the substring form alone.

**Still on substring matching:** the `uui-card-media` and `uui-card-block-type` locators (`ContentUiHelper`, `LibraryUiHelper`, `MediaUiHelper`, `DataTypeUiHelper`, `UiBaseLocators`) still filter with `{hasText: name}` — 14 sites, budgeted. Read those two card components first; if they expose an attribute or a light-DOM text node for the name, convert on that basis rather than guessing.

`{hasText: ...}` remains correct for **structural** filtering — narrowing to a group, tab, property or box by its label (`filter({hasText: 'Document permissions'})`). The rule is about entity names, which are the values leftover data collides on.

---

## 4. Test data & isolation

- **Idempotent cleanup**: create with the API, tear down with `ensureNameNotExists()` in both `beforeEach` and `afterEach`.
- **Tests run serially** (`workers: 1`) because specs share fixed entity names and would collide in parallel.

### Entity names are shared, and that is fine — for a reason worth knowing

Name sharing is not the exception, it is the norm: **167 name constants appear in more than one spec**, `TestDocumentTypeForContent` in 92 files and `TestContent` in 79. Per-file uniqueness is not what keeps the suite safe. Three other things do:

1. **`workers: 1`** — nothing runs concurrently, so a shared name is only ever in use by one spec at a time.
2. **Creates clear the name first** — **279 of the 406 `create*` helpers** call `ensureNameNotExists` as their first act, so a leftover from a crashed run is removed by the next spec that wants that name.
3. **Teardown is idempotent** in both hooks, so running it twice, or on something already gone, is harmless.

The residual risk sits with the **127 `create*` helpers that do not ensure first**: for those, a shared name plus a leftover means a duplicate or a 400 rather than a clean overwrite. If you add a `create*` helper, ensure the name first — that is the habit the suite actually depends on.

So the rule that matters is not "unique names" but "clean up anything global" — see the teardown table below, which `npm run audit` enforces.
- Cleanup caveats to be aware of when debugging leftover state: some `ensureNameNotExists` / `recurseChildren` helpers delete only the **first** match, and list fetches use a single large `take` (no pagination) — duplicates or very large trees can leave residue.

### What actually has to be torn down

**Deleting a type removes its instances**, so a spec that cleans up the document type does not also need to remove its documents — and most specs rely on exactly that. The cascades: `document`, `documentBlueprint` and `element` from `documentType`; `media` from `mediaType`; `member` from `memberType`.

**Everything else has to be removed explicitly**, and these are the ones that bite because they are *global*: a `language`, `userGroup`, `user`, `memberGroup`, `dataType`, `template`, `dictionary`, `webhook` or `relationType` left behind changes what every later spec sees. `npm run audit` checks this (`entityNotTornDown`, budget 0), with the cascade map built in so it does not fire on the common case.

Five real leaks existed when the rule was written, and the shape is instructive — each had a sibling spec doing it correctly:

| Spec | Leaked | The sibling that got it right |
|------|--------|-------------------------------|
| `BlockGridWithPropertyEditor` | the Danish `language` | `ContentWithBlockGrid` removes it |
| `BlockListWithPropertyEditor` | the Danish `language` | `ContentWithBlockList` removes it |
| `DocumentPropertyValuePermissionInBlock` | `userGroup` + test `user` | `ContentStartNodes` removes both |
| `ListView` | a `documentType` named `TestDocumentType` | — |
| `DefaultPermissionsInContent` | a `memberGroup` named `TestMemberGroup` | — |

Note the last two: the leaked names (`TestDocumentType`, `TestMemberGroup`) are generic enough that other specs use them for their own fixtures, which is what turns a leak into a cross-spec failure rather than harmless residue.

### A disabled test carries an annotation, not a comment

64 of the suite's 1636 tests are off (`test.skip` / `test.fixme`). That is a standing claim about what the product is *not* covered for, so it has to be answerable without grepping. Every one carries a machine-readable annotation:

```ts
test.skip('can create content with block grid area with min allowed',
  {annotation: {type: 'issue', description: 'Skip this test due to this issue: https://github.com/umbraco/Umbraco-CMS/issues/22121'}},
  async ({umbracoApi, umbracoUi}) => {
```

Annotations show up in every reporter (and in the HTML report's test detail), so `--reporter=json` answers "what are we not testing, and what is it blocked on?" directly. A comment above the test does not.

| `type` | Means | Current |
|--------|-------|---------|
| `issue` | Blocked on a tracked GitHub issue or PR — description carries the link | 25 |
| `blocked` | Blocked on product behaviour with no issue filed yet ("the front-end does not support…") | 34 |
| `todo` | Never implemented — the body is an empty stub | 3 |
| `fixme` | Fully implemented, disabled, reason never recorded | 2 |

Rules: a new disabled test needs an annotation (`npm run audit` fails without one), and prefer `type: 'issue'` with a link — a `blocked` entry carrying only prose is how a test stays off for two years.

The two categories that are not really "skipped tests" at all:

- **`todo` (3)** — `ContentWithBlockGrid`/`ContentWithBlockList` "can move blocks in the content" and `User` "can change from grid to table view". The body is `// TODO: Implement it later`. A skipped empty stub asserts nothing and documents nothing; write it or delete it.
- **`fixme` (2)** — both in `BlockGridEditor` (moving a block between groups, deleting a group). These have complete arrange/act/assert bodies, so something once worked and then didn't. They need one run against a current build to decide product bug vs stale test.

If a test is off because the *feature* was removed, delete it — a permanent skip is not documentation.

---

## 5. Builders

`lib/builders/` (114 files) builds the JSON payloads specs send to the Management API. Two conventions hold across all of it and should keep holding:

- **Fluent setters return `this`; nested setters return the sub-builder.** `withX()` returns `this`; `addX()` returns a new child builder that ends with `.done()` to climb back. All 68 `addX()` methods follow this — there are no broken chains.
- **There are four exit names, and the distinction is real:** `build()` (48 files) is the payload a spec sends; `getValues()` (50) is the `DataTypeBuilder` hook returning the `{alias, value}` array; `getValue()` (13) is a value sub-builder returning one property value; `done()` (62) climbs back to the parent. A class with none of the first three is unfinished — but check for `getValue()` before concluding that.

### Client-side id generation is not uniform

13 builders call `ensureIdExists` in `build()` and so emit a client-generated GUID — `document`, `element`, `member`, `dataType`, the three content types, and the property/area/group sub-builders. **`media`, `userGroup` and `user` do not**: they emit `id: null` and let the server assign, even though they are the same category of top-level entity builder.

Both work, because `ApiHelpers.create()` reads the new id from the `Location` header either way. But the point of `ensureIdExists` is that a spec can know an entity's id *before* creating it, and for those three you cannot. Aligning them is a payload change across many specs, so it wants verifying against a running instance rather than doing blind — `npm run helpers:selftest` pins the current split so a change to either side is deliberate.

Sub-builders that reference an existing entity (`*AllowedDocumentTypeBuilder`, `*ContainerBuilder`, the permission builders) correctly do **not** generate ids.

### Typing the payloads

The builders are the boundary between specs and the Management API, and it used to be an unchecked one: a payload with a misspelled or missing field compiled happily and came back as an opaque 400, which reads as a confusing test failure rather than a compile error.

`lib/builders/types.ts` now declares the payload envelope (`EntityValue`, `EntityReference`, `DataTypeValues`, `DataTypePayload`). These type the **envelope**, not every leaf: a property `value` is genuinely heterogeneous (string, number, nested block structure) so it stays `any`, while the field names and nesting the API requires are checked. That is where the real mistakes are.

`DataTypeBuilder` — the one abstract base, with 36 subclasses — is wired up:

```ts
build(): DataTypePayload
abstract getValues(): DataTypeValues
```

**The part that is easy to get wrong:** annotating the base class alone achieves nothing. Every subclass declared `let values: any[] = []` and pushed into it, so the `any` swallowed the mistake long before it reached the return type — the annotation type-checked and caught precisely zero real errors. The 33 subclasses now declare `const values: DataTypeValues = []`, and with that in place a misspelled `allias:` fails compilation with *"Did you mean to write 'alias'?"*. **An `any` anywhere on the path to `build()` defeats the whole exercise**, which is why `npm run audit` budgets `: any` inside builders alongside the untyped exits.

Still to do: 96 `build()`/`getValues()` outside the `DataTypeBuilder` hierarchy have no declared return type, and 25 `: any` remain (mostly the `*ValueBuilder` classes for documents, media and elements). Both are budgeted. Worth typing per-entity as each builder is touched — define the payload interface in `types.ts`, annotate the exit, then remove the `any` that would otherwise defeat it.

### The bug class to look for in a builder

**A setter whose value never reaches the payload.** It is invisible: the spec compiles, the chain reads correctly, the test passes, and the API simply never receives the value. `withEntityType()` was exactly this on three value builders — the payload's `entityType` was computed from `this.editorAlias`, a different field, so every call was discarded. `MediaApiHelper` had been asking for `'media-property-value'` and sending `null`.

It survives because the key name and the field name differ *legitimately* almost everywhere else — `parent: this.parentId ? {id: …}`, `min: this.minValue`, `value: this.lineNumbers` — so a mismatch does not look wrong on its own. What made this one a bug is that a field of the same name existed and was ignored.

When adding or reviewing a setter, check the field is read in the exit method, and add a case to `npm run helpers:selftest` asserting the built payload carries it. Nothing else catches this: not `tsc`, not the audit, and not a green test run.

**The same class exists one layer up, in the helpers: a parameter the body never reads.** The audit checks for it (`unusedHelperParam`), and the shape to fear is a negation flag — an ignored `isVisible: boolean = true` means a caller passing `false` still gets the *positive* assertion, so the test asserts the opposite of what it reads. The suite currently has **zero** of those; the one finding is a vestigial `pageDocumentTypeAlias` on `TemplateApiHelper.createTemplateWithDisplayingElementPickerVarianceAndIdentityMethods`, copy-pasted from a sibling that does use it to emit a `PageIsDocumentType` line. Nothing asserts that line, so nothing fails — but a spec that passed the argument expecting it would be silently disappointed.

---

## 6. Generated / ignored files

- `console-errors.json` is generated at install/run time and is **git-ignored** — do not commit it. Console errors captured during a run are appended here for inspection; they are not (yet) a failing gate.
- `.env`, `playwright/.auth/`, and `results/` are also ignored.
- `dist/` is the `npm run build` output (`lib/` only) and is what the npm package ships.
- `.prettierrc.json` has **no matching prettier dependency and no format script**, so nothing in CI enforces it. That is deliberate, not an oversight: an editor's Prettier extension reads the file without a local install, which is what it is there for. The same pattern holds in `src/Umbraco.Web.UI.Login`; only `src/Umbraco.Web.UI.Client` actually depends on prettier. Don't delete the file expecting it to be dead, and don't add the dependency without deciding you want a formatting gate — turning one on would rewrite a large part of the suite in one commit.

---

## 7. The audit — these conventions are checkable

There is no lint step, so §3 used to be enforced by memory alone. `audit-conventions.js` is the checker:

```bash
npm run audit              # one line per rule
npm run audit -- --verbose # every finding, with file:line
```

It needs no Umbraco instance and exits non-zero when a rule goes over budget.

**How the budgets work.** Rules that should never be violated have a budget of **0** and fail the moment one appears. Rules that are pre-existing debt carry the current count as their budget, so the count can only shrink — the ratchet stops the debt growing without demanding it be paid off today.

**A rule must never contradict the convention it enforces.** The sleep and force-click rules originally counted *every* occurrence, justified or not, with the budget set to the exact current total. §3 permits either escape when there is genuinely no observable state *provided a comment says what it stands in for* — so a properly justified addition pushed the count over budget, and the only way out was raising the budget, which this section forbids. The convention was unfollowable. Both rules now count only the **unjustified** ones; the comment is the contract, and the budget tracks the sites that lack one. `literalIndexLocator` works the same way: a parameterised `.nth(i)` is the legitimate form and is not counted, only a hardcoded literal is. If you add a rule with a documented exception, exclude that exception in the rule itself and add a `good` self-test case proving it.

| Rule | Budget | Why it matters |
|------|-------:|----------------|
| Dropped promises | 0 | assertion never runs; test greens regardless |
| Value-returning check nothing asserts on | 0 | same silent green, different disguise |
| Assert-internally helper called without `await` | 0 | failure becomes a rejected promise; the test reports as passed |
| Endpoint constant absent from `OpenApi.json` | 0 | a renamed route becomes a mystifying timeout |
| Spec creates an entity it never tears down | 0 | global residue changes what later specs see |
| Spec outside a project directory | 0 | file is never run at all |
| Raw `page` fixture in a spec | 0 | bypasses the page objects |
| Hardcoded API endpoint | 0 | belongs in `ConstantHelper.apiEndpoints` |
| Disabled test without an annotation | 0 | invisible in reports |
| Fixed sleep **without a justification** | 81 | debt — see §3 |
| `force: true` **without a justification** | 79 | debt — masks actionability failures |
| Entity name matched on a substring | 14 | strict-mode multi-match on leftover data |
| Raw `.click()` in `lib/` | 5 | skips the visibility wait |
| Hardcoded `.nth(N)` | 15 | bakes in unpromised list order |
| Hardcoded timeout in ms | 1 | belongs in `ConstantHelper.timeout` |
| Spec reaching through a helper to `page` | 11 | navigation belongs in a helper |
| Untyped builder `build()`/`getValues()` | 96 | malformed payload compiles, fails as a 400 |
| `: any` inside a builder | 25 | defeats the payload types downstream |
| Commented-out test | 17 | invisible to `--list` and every reporter |
| Commented-out assertion in a live test | 15 | test asserts less than it appears to |
| TODO with no version trigger or author | 15 | cannot rot out loud, so never gets removed |
| Spec asserting on a raw API response shape | 211 | couples 269 files to the response shape |
| Deprecation with no removal version | 0 | consumer has no runway; we never know when to delete |
| Helper parameter the body never reads | 1 | signature promises what the body does not do |

**When you reduce a count, lower the budget in the same commit** — the audit prints the new number for you. Never raise a budget to make a run pass; that is the one move that turns a ratchet back into a wish.

**CI runs both gates.** `build/azure-pipelines.yml`, `Build` stage, job C ("Build Test Helpers Package") runs `npm run typecheck` then `npm run audit` before packing — that job already installs dependencies and neither gate needs an Umbraco instance. Before this, nothing in CI type-checked or linted this project at all, which is how a wrong package name and 95 sleeps accumulated unnoticed.

The audit is a text scanner, not a type checker — it catches shapes, not semantics. It is a floor under §3, not a substitute for reading it.

### The audit tests itself

**`npm run audit` runs `audit-selftest.js` first and refuses to report anything if it fails.** This is not ceremony. Six rules gate at budget 0, and for those a rule whose regex silently stops matching looks *exactly* like a rule that is passing — it reports `clean` forever while the convention goes unenforced. Two of these detectors shipped with real bugs on their first draft (a dropped-promise rule with 19 false positives; a commented-assertion rule that double-counted a wholly-dead file), so this is a demonstrated failure mode.

The self-test builds a throwaway fixture tree per case and points the audit at it with `--root`. Every rule needs two kinds of case:

- a **`bad`** fixture it must flag — proves the rule still detects;
- a **`good`** fixture it must ignore — proves it doesn't fire on the idiom it is meant to permit (an anchored TODO, `this.click()`, a multi-line `await expect(...).toPass()`).

It also **fails if any rule has no case at all**, so adding a rule without a test is caught at once. Add both cases in the same commit as a new rule; a rule with no `good` case is how a checker starts flagging correct code and gets switched off.

Run it alone with `npm run audit:selftest`.

---

## 8. Comments and documentation

The root `CLAUDE.md` §9 comment policy applies here in full — default to no comment; write one for a non-obvious *why*, an invariant the types don't enforce, or an edge case deliberately handled.

**JSDoc coverage is 3% (71 of 2671 public helper methods) and that is correct, not a gap.** `clickSaveAndPublishButton()` and `enterElementName(name)` say what they do; a `/** Clicks the save and publish button. */` above them is the noise §9 tells you not to write. JSDoc in `lib/` sits exactly where the contract is *not* obvious — `BasePage`'s primitives (what does `isVisible(locator, isVisible)` actually assert?), the response-waiting helpers, `BuilderUtils`, and the two assertion helpers in §3. Keep it that way: document the surprising, not the self-evident. Don't run a coverage sweep.

**Three comment shapes are never right**, and `npm run audit` counts all three:

- **A commented-out test.** The most thoroughly hidden form of disabled test — invisible to `--list`, to every reporter, and to the annotation rule in §4. `tests/DefaultConfig/Packages/CreatedPackages.spec.ts` is 347 lines and 17 tests commented out wholesale behind `// UNCOMMENT WHEN FIXED`, with no issue link; it has been dead since the **v15** era and contributes 0 of the suite's 1636 tests. Use `test.skip` with an annotation instead, so a disabled test is at least countable.
- **A commented-out assertion in a live test.** Strictly worse than deleting it: the test still passes while quietly checking less than it appears to. There are 15, eight of them in `UserGroupsDefaultConfiguration.spec.ts`. Restore it or delete it.
- **An unanchored TODO.** §9 allows TODOs precisely because they are deleted when done — which needs an anchor to hang off: `// TODO (V19): remove once the obsolete overload is gone` or `// TODO: pagination [NL]`. A bare `// TODO: Implement it later` (15 of these) can't rot out loud, so it never gets removed.

### Keeping the docs true

Three rules, each learned from a way this file and `README.md` went stale:

- **Don't restate what a config file owns.** Point at it, or document only what it can't say — *why* `workers: 1`, not *that* it is 1. The README carried a 30s test timeout against a config saying 60s, and "2 retries on CI, 0 locally" against `retries: 2` unconditionally. A number living in two places disagrees eventually.
- **Give debt a count and a location, not an adjective.** "Prefer deterministic waits" coexisted with 95 sleeps for as long as it named no number. "91, worst in `ContentUiHelper`, `LibraryUiHelper`, `UiBaseLocators`" can be checked — and §7 now checks it.
- **A rename is a doc change.** The npm package moved to the `@umbraco-cms` scope and reached none of the three READMEs, so the consumer-facing `npm install` line was wrong for months. Any change to a published name, script, or path sweeps the docs in the same PR.

---

## 9. Skill

`/umb-e2e-test` (`.claude/skills/umb-e2e-test/`) wraps this document into a working procedure — where a spec belongs, which layer a new locator goes in, the determinism checklist to apply before committing, and the verification gates. Reach for it when writing or repairing a spec or a helper; it is the executable form of §3.
