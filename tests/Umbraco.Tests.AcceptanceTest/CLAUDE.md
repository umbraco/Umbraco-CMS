# Umbraco Acceptance Tests — Contributor & Agent Guide

Playwright end-to-end tests for Umbraco CMS. This file covers **how the suite is built and the conventions that keep it deterministic**. For setup, running, and writing your first test, see [README.md](./README.md) first — this document does not repeat it.

---

## 1. The one check you can run without an Umbraco instance

```bash
npm run typecheck   # tsc -p tsconfig.json --noEmit — lib/ AND tests/
```

Run it before committing; it needs no running site, so there is no excuse to skip it.

`npm run build` compiles `lib/` only (`tsconfig.build.json`) and does **not** type-check the specs under `tests/` — that is what `npm run typecheck` is for.

**There is no lint step, and nothing mechanically enforces §3.** Every convention in it is applied by reading and by review. That is worth knowing before you rely on a green `typecheck` meaning more than it does: it proves the types line up, not that a promise is awaited, an entity is torn down, or a locator matches exactly one element.

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

Nothing enforces these mechanically. They hold because they are read and applied.

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

**Known debt — the suite has not finished paying this off.** 91 `waitForTimeout` calls remain, concentrated in `ContentUiHelper`, `LibraryUiHelper` and `UiBaseLocators`. Only **10 of them carry a justification**; the other 81 do not. Treat them as debt, not as precedent: the fact that a neighbouring method sleeps is not a reason for a new one to.

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
`BasePage.click()` awaits `toBeVisible` before clicking. Use it — that wait is the whole point, and a click on an element that has not rendered yet is a flake with a misleading message.

Two cases legitimately click raw, and the audit does not count either:

- **The receiver was already awaited visible.** `hoverAndClick` asserts `toBeVisible` on both locators before clicking, so wrapping it again would add nothing.
- **The click needs an option `this.click()` cannot express.** It takes only `{force, timeout}`, so a middle-click (`clearTipTapEditor` uses one to avoid opening a block in the RTE) or a modifier click has no wrapper available. Wait for visibility yourself on the line before.

Anything else is counted, at a budget of **0**. What the rule looks for is a click with no visibility wait, not the spelling of the call — it reads back a few lines for a `toBeVisible` or `waitForVisible` naming the same receiver. Its previous version excluded any line matching `locator.click`, which exempted a call purely because its variable was named `locator`, and four of its five findings were false positives while the one real case — a chained `.filter(...).locator(...).click()` with no wait at all — sat in the same list.

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

Both halves of this were swept once and were clean at the time: every one of the ~730 migrated call sites is awaited, and the 12 value-returning checks are all wrapped in `expect()`. Neither property is checked automatically, so both are worth a glance in review.

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

**Think hard before changing any of these semantics** — these helpers back ~700 spec assertions, so a change to what they mean silently changes what all of those specs assert, with no failure to point at the cause.

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

**184 raw-response assertions remain**, down from 890 (-79%), and the count is budgeted.

The two `get*` helpers are what made the deep cases tractable. `getOnlyPropertyValue` in particular replaces `values[0].value` where no alias is in play: it asserts there *is* exactly one property and returns it, so the single-property expectation is stated instead of buried in an index — and a spec that later grows a second property fails loudly rather than silently asserting against whichever sorts first. Both stop at the entity boundary: `getOnlyPropertyValue(data).contentData[0].values[0].value` still indexes into the *block's own* structure, which is that test's actual subject.

**184 remain, and the previous read of them was wrong** — worth knowing, because the mistake is easy to repeat. The conclusion had been "no shape appears more than nine times, spread across roughly twenty-five distinct ones, so writing a helper for each would inflate the package for little". That is true of **exact paths** — there are 86, and the largest is `.id` at ten. It is false of **subjects**: cluster by what is actually being asserted and 86 paths collapse to 13, several of them large and concentrated in two or three files:

| Subject | Lines | Files |
|---------|------:|------:|
| property definitions (name, description, dataType, validation, appearance) | 23 | 3 |
| identity (id / key / path / alias / name) | 22 | 16 |
| user-group access flags and permissions | 22 | 2 |
| compositions | 14 | 4 |
| allowed templates and child types | 13 | 6 |
| domains | 13 | 2 |
| property values | 12 | 7 |
| file content | 9 | 2 |
| variants, member fields, containers, child variants, pagination | 34 | — |

**Concentration is what decides whether a helper earns its place, not the raw count.** Twenty-two `.id` assertions across sixteen files are a genuine long tail — a helper would add an indirection per file and buy nothing. Twenty-three property-definition assertions across *three* files are not: they were all `properties[0].<field>`, one positional lookup repeated, which is exactly what `getPropertyValue` was introduced to remove on the values side. That cluster is now `getPropertyDefinition(data, alias)` and `getOnlyPropertyDefinition(data)`.

The next two worth doing on the same grounds are **user-group access flags** (22 lines, 2 files) and **domains** (13 lines, 2 files). The rest are long tail: add a helper when you touch one and it earns its place, and do not add new raw assertions, since the audit will fail.

**One index stays on purpose.** The three "can reorder properties" tests assert `properties[0].name` and `properties[1].name` — there the position *is* the subject, the same exemption a parameterised `.nth(i)` gets. Two of the three asserted only `properties[0]`, which cannot tell a reorder from a property being dropped or duplicated; both now assert the second position too, matching the document-type test that had it right.

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
`.nth(i)` is right for "the i-th block" when the index is a parameter and the position is what the test is about — 110 of the suite's 124 uses are that. A **hardcoded** `.nth(0)` / `.nth(2)` is different: it bakes in an assumption about list order that neither the backend nor the seed data promises, and produces exactly the coincidental pass the root `CLAUDE.md` §10 warns about. There are 15. Prefer a locator that names what it wants (a label, a `[name=...]`, an exact text); reach for a literal index only when nothing distinguishes the elements.

**Nine of the 15 cannot be checked from this repository**, and it is worth knowing why before trying. They are in `FormsUiHelper`, against `forms-settings-validation` and friends — Umbraco **Forms** elements, whose source is not in this repo. The other locator conventions in this section are verifiable statically because the backoffice source sits under `src/Umbraco.Web.UI.Client/` and `@umbraco-ui/uui` ships its compiled elements; neither is true for Forms. So for those nine the positional index is all there is without a running Forms install, and converting them blind is guessing at which element gets targeted.

The remaining six (`DataTypeUiHelper`, `RelationTypeUiHelper`, `UserGroupUiHelper`) target components that *are* readable, but replacing a positional locator there changes which element the helper acts on — so they want one run to confirm, not a static edit.

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

`ConstantHelper.apiEndpoints` hardcodes 46 Management API paths, and 232 more are written inline in the `*ApiHelper` files. Nothing ties them to the API, so a renamed route surfaces as a helper waiting for a response that never arrives — a 60-second timeout with no hint of the cause. When a wait times out for no visible reason, check the path against the committed `src/Umbraco.Cms.Api.Management/OpenApi.json`, which is the contract and needs no running instance. Seven paths had already drifted when this was last checked by hand.

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

**The cards are converted, and they needed two different forms** — which is the clearest illustration of why this section says to read the component. Three shared locators on `UiBaseLocators` now hold the difference:

| Card | Binds the name as | Locator |
|------|-------------------|---------|
| `uui-card-media` | an **attribute** (`name=${item.name}`) | `getMediaCardWithName` → `uui-card-media[name="..."]` |
| `uui-card-block-type` | a **property** (`.name=`), not reflected | `getBlockTypeCardWithName` → exact text |
| `uui-card-user` | a **property** (`.name=`), not reflected | `getUserCardWithName` → exact text |

Only the media card can be matched on the attribute. The other two are bound with Lit's `.name=` property syntax and neither component declares `reflect: true`, so **no `name` attribute exists in the DOM at all** — `[name="..."]` there matches nothing and the failure looks like a missing element rather than a wrong locator. Both render `<span title=${name}>${name}</span>` in their shadow root, so exact text is the form that fits.

Two things made this checkable without a running site: `@umbraco-ui/uui` ships `vscode.html-custom-data.json` and its compiled elements under `node_modules`, which is where the reflection question is answered; and the substring form being replaced already matched that shadow text, which proves the text is reachable, leaving only substring-vs-exact to change.

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

So the rule that matters is not "unique names" but "clean up anything global" — see the teardown table below.
- Cleanup caveats to be aware of when debugging leftover state: some `ensureNameNotExists` / `recurseChildren` helpers delete only the **first** match, and list fetches use a single large `take` (no pagination) — duplicates or very large trees can leave residue.

### What actually has to be torn down

**Deleting a type removes its instances**, so a spec that cleans up the document type does not also need to remove its documents — and most specs rely on exactly that. The cascades: `document`, `documentBlueprint` and `element` from `documentType`; `media` from `mediaType`; `member` from `memberType`.

**Everything else has to be removed explicitly**, and these are the ones that bite because they are *global*: a `language`, `userGroup`, `user`, `memberGroup`, `dataType`, `template`, `dictionary`, `webhook` or `relationType` left behind changes what every later spec sees. Check this by eye when you add a `create*` to a spec: does something in the same file remove it, or remove a type it cascades from?

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

79 of the suite's 1651 tests are off (`test.skip` / `test.fixme`). That is a standing claim about what the product is *not* covered for, so it has to be answerable without grepping. Every one carries a machine-readable annotation:

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
| `fixme` | Fully implemented but disabled | 17 |

Rules: a new disabled test needs an annotation, and prefer `type: 'issue'` with a link — a `blocked` entry carrying only prose is how a test stays off for two years.

The two categories that are not really "skipped tests" at all:

- **`todo` (3)** — `ContentWithBlockGrid`/`ContentWithBlockList` "can move blocks in the content" and `User` "can change from grid to table view". The body is `// TODO: Implement it later`. A skipped empty stub asserts nothing and documents nothing; write it or delete it.
- **`fixme` (17)** — two in `BlockGridEditor` (moving a block between groups, deleting a group) with complete arrange/act/assert bodies, so something once worked and then didn't; both need one run against a current build to decide product bug vs stale test. The other fifteen are `CreatedPackages`, which had been commented out wholesale since the v15 era: uncommenting them changed nothing about what runs, but it moved 15 tests from invisible to countable, which is why the suite total went from 1651 to 1651. Enable them one at a time against a running instance.

If a test is off because the *feature* was removed, delete it — a permanent skip is not documentation.

---

## 5. Builders

`lib/builders/` (114 files) builds the JSON payloads specs send to the Management API. Two conventions hold across all of it and should keep holding:

- **Fluent setters return `this`; nested setters return the sub-builder.** `withX()` returns `this`; `addX()` returns a new child builder that ends with `.done()` to climb back. All 68 `addX()` methods follow this — there are no broken chains.
- **There are four exit names, and the distinction is real:** `build()` (48 files) is the payload a spec sends; `getValues()` (50) is the `DataTypeBuilder` hook returning the `{alias, value}` array; `getValue()` (13) is a value sub-builder returning one property value; `done()` (62) climbs back to the parent. A class with none of the first three is unfinished — but check for `getValue()` before concluding that.

### Client-side id generation is not uniform

13 builders call `ensureIdExists` in `build()` and so emit a client-generated GUID — `document`, `element`, `member`, `dataType`, the three content types, and the property/area/group sub-builders. **`media`, `userGroup` and `user` do not**: they emit `id: null` and let the server assign, even though they are the same category of top-level entity builder.

Both work, because `ApiHelpers.create()` reads the new id from the `Location` header either way. But the point of `ensureIdExists` is that a spec can know an entity's id *before* creating it, and for those three you cannot. Aligning them is a payload change across many specs, so it wants verifying against a running instance rather than doing blind.

Sub-builders that reference an existing entity (`*AllowedDocumentTypeBuilder`, `*ContainerBuilder`, the permission builders) correctly do **not** generate ids.

### Typing the payloads

The builders are the boundary between specs and the Management API, and it used to be an unchecked one: a payload with a misspelled or missing field compiled happily and came back as an opaque 400, which reads as a confusing test failure rather than a compile error.

`lib/builders/types.ts` now declares the payload envelope (`EntityValue`, `EntityReference`, `DataTypeValues`, `DataTypePayload`). These type the **envelope**, not every leaf: a property `value` is genuinely heterogeneous (string, number, nested block structure) so it stays `any`, while the field names and nesting the API requires are checked. That is where the real mistakes are.

`DataTypeBuilder` — the one abstract base, with 36 subclasses — is wired up:

```ts
build(): DataTypePayload
abstract getValues(): DataTypeValues
```

**The part that is easy to get wrong:** annotating the base class alone achieves nothing. Every subclass declared `let values: any[] = []` and pushed into it, so the `any` swallowed the mistake long before it reached the return type — the annotation type-checked and caught precisely zero real errors. The 33 subclasses now declare `const values: DataTypeValues = []`, and with that in place a misspelled `allias:` fails compilation with *"Did you mean to write 'alias'?"*. **An `any` anywhere on the path to `build()` defeats the whole exercise** — so when you touch a builder, check the local the exit returns is declared, not just the exit.

The remaining debt is not in those 33 subclasses — it is in the **sub-builders**, which return an inline object literal or push into a `let values: any = {}`. There a declared return type does do the work: TypeScript's excess-property check fires on a literal in a return position and names the mistake. That is what the audit's `untypedBuilderExit` now counts, so paying it off means giving each sub-builder's item shape an interface — not sprinkling annotations on the exits that are already checked.

**What is typed now.** `types.ts` carries the payload envelopes: `EntityVariant` and
`EntityPropertyValue` for the five variant and five value builders; `DocumentPayload`,
`MediaPayload` and `MemberPayload` for the entity builders; `DocumentTypePayload`,
`MediaTypePayload` and `MemberTypePayload` for the three content-type builders, sharing
`ContentTypePayloadBase`. The container, property and composition shapes inside those are
`ReturnType<typeof buildContainer>` and friends, derived from `BuilderUtils` rather than
restated, so there is one definition to change.

Two shapes are deliberately not normalised, because the payload is what the suite has always
sent and changing it wants a running instance rather than a tidy-up: `MemberVariantBuilder`
sends `name: ''` where the other four send `null`, and `MemberValueBuilder` omits `editorAlias`
and `entityType` entirely (hence both optional on `EntityPropertyValue`).

The sub-builders are done too — block grid, block list, tiptap, TinyMCE, list view, image
cropper, media picker, user-group permissions — so **`untypedBuilderExit` is a gate now, not
debt**: a new exit returning an unchecked shape fails the build.

Most of those built their payload conditionally into a `let values: any = {}` and returned it,
and that is the shape to know, because the fix is not the return type. **The local declaration
is what does the work.** With `const values: BlockGridBlockConfiguration = {}`, a
`values.allowAtRot = …` is an error that names the typo; on an `any` local the identical line
is silent, and a return annotation over it changes nothing. So these interfaces have every
property optional — the builders emit a field only when it is set — and the local carries the
type.

One case needed more than an annotation: `TiptapBlockBuilder` assigned with brackets
(`values['label'] = …`). Bracket assignment is **not** checked, because `noImplicitAny` is off
under `strict: false`, so it was converted to dot access. Same payload, and the typo in
`displayInline` it now catches is the kind with a lowercase L in it.

`: any` is down from 25 to 12, and the remaining 12 split two ways. Nine are correct: a
property `value` is genuinely heterogeneous, and the four `let value: any = null` accumulators
hold one. Three are exported signatures that ought to narrow — two `withCulture(culture: any)`
against a sibling that already declares `string | null`, and `withEditorMode(editorMode: any)`
whose only caller passes `'Classic'` — but §2 forbids changing an exported signature in a
minor, so each carries a `TODO (V19)` rather than a change.

When you add a sub-builder, follow the same order: declare the shape in `types.ts` with
optional properties, declare the **local** with it, annotate the exit, then plant a misspelled
field and confirm `tsc` names it. That last step is not optional — it is the only thing that
distinguishes typing that works from typing that looks like it does.

### The bug class to look for in a builder

**A setter whose value never reaches the payload.** It is invisible: the spec compiles, the chain reads correctly, the test passes, and the API simply never receives the value. `withEntityType()` was exactly this on three value builders — the payload's `entityType` was computed from `this.editorAlias`, a different field, so every call was discarded. `MediaApiHelper` had been asking for `'media-property-value'` and sending `null`.

It survives because the key name and the field name differ *legitimately* almost everywhere else — `parent: this.parentId ? {id: …}`, `min: this.minValue`, `value: this.lineNumbers` — so a mismatch does not look wrong on its own. What made this one a bug is that a field of the same name existed and was ignored.

When adding or reviewing a setter, check by hand that the field is read in the exit method. Nothing catches this otherwise: not `tsc`, and not a green test run.

**The same class exists one layer up, in the helpers: a parameter the body never reads.** The audit checks for it (`unusedHelperParam`), and the shape to fear is a negation flag — an ignored `isVisible: boolean = true` means a caller passing `false` still gets the *positive* assertion, so the test asserts the opposite of what it reads. The suite currently has **zero** of those; the one finding is a vestigial `pageDocumentTypeAlias` on `TemplateApiHelper.createTemplateWithDisplayingElementPickerVarianceAndIdentityMethods`, copy-pasted from a sibling that does use it to emit a `PageIsDocumentType` line. Nothing asserts that line, so nothing fails — but a spec that passed the argument expecting it would be silently disappointed.

---

## 6. Generated / ignored files

- `console-errors.json` is generated at install/run time and is **git-ignored** — do not commit it. Console errors captured during a run are appended here for inspection; they are not (yet) a failing gate.
- `.env`, `playwright/.auth/`, and `results/` are also ignored.
- `dist/` is the `npm run build` output (`lib/` only) and is what the npm package ships.
- `.prettierrc.json` has **no matching prettier dependency and no format script**, so nothing in CI enforces it. That is deliberate, not an oversight: an editor's Prettier extension reads the file without a local install, which is what it is there for. The same pattern holds in `src/Umbraco.Web.UI.Login`; only `src/Umbraco.Web.UI.Client` actually depends on prettier. Don't delete the file expecting it to be dead, and don't add the dependency without deciding you want a formatting gate — turning one on would rewrite a large part of the suite in one commit.

---

## 7. Comments and documentation

The root `CLAUDE.md` §9 comment policy applies here in full — default to no comment; write one for a non-obvious *why*, an invariant the types don't enforce, or an edge case deliberately handled.

**JSDoc coverage is 3% (71 of 2671 public helper methods) and that is correct, not a gap.** `clickSaveAndPublishButton()` and `enterElementName(name)` say what they do; a `/** Clicks the save and publish button. */` above them is the noise the root §9 tells you not to write. JSDoc in `lib/` sits exactly where the contract is *not* obvious — `BasePage`'s primitives (what does `isVisible(locator, isVisible)` actually assert?), the response-waiting helpers, `BuilderUtils`, and the two assertion helpers in §3. Keep it that way: document the surprising, not the self-evident. Don't run a coverage sweep.

**Three comment shapes are never right:**

- **A commented-out test.** The most thoroughly hidden form of disabled test — invisible to `--list`, to every reporter, and to the annotation rule in §4. `tests/DefaultConfig/Packages/CreatedPackages.spec.ts` is 347 lines and 17 tests commented out wholesale behind `// UNCOMMENT WHEN FIXED`, with no issue link; it has been dead since the **v15** era and contributes 0 of the suite's 1651 tests. Use `test.skip` with an annotation instead, so a disabled test is at least countable.
- **A commented-out assertion in a live test.** Strictly worse than deleting it: the test still passes while quietly checking less than it appears to. There are 15, eight of them in `UserGroupsDefaultConfiguration.spec.ts`. Restore it or delete it.
- **An unanchored TODO.** The root §9 allows TODOs precisely because they are deleted when done — which needs an anchor to hang off: `// TODO (V19): remove once the obsolete overload is gone` or `// TODO: pagination [NL]`. A bare `// TODO: Implement it later` (15 of these) can't rot out loud, so it never gets removed.

### Keeping the docs true

Three rules, each learned from a way this file and `README.md` went stale:

- **Don't restate what a config file owns.** Point at it, or document only what it can't say — *why* `workers: 1`, not *that* it is 1. The README carried a 30s test timeout against a config saying 60s, and "2 retries on CI, 0 locally" against `retries: 2` unconditionally. A number living in two places disagrees eventually.
- **Give debt a count and a location, not an adjective.** "Prefer deterministic waits" coexisted with 95 sleeps for as long as it named no number. "91, worst in `ContentUiHelper`, `LibraryUiHelper`, `UiBaseLocators`" can be checked by anyone reading it.
- **A rename is a doc change.** The npm package moved to the `@umbraco-cms` scope and reached none of the three READMEs, so the consumer-facing `npm install` line was wrong for months. Any change to a published name, script, or path sweeps the docs in the same PR.

---

## 8. Skill

`/umb-e2e-test` (`.claude/skills/umb-e2e-test/`) wraps this document into a working procedure — where a spec belongs, which layer a new locator goes in, the determinism checklist to apply before committing, and the verification gates. Reach for it when writing or repairing a spec or a helper; it is the executable form of §3.
