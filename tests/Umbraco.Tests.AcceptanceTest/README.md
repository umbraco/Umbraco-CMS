# Umbraco Acceptance Tests

End-to-end acceptance tests for Umbraco CMS using [Playwright](https://playwright.dev/).

You can watch a video following these instructions [here](https://www.youtube.com/watch?v=N4hBKB0U-d8) and a longer UmbraCollab recording [here](https://www.youtube.com/watch?v=hvoI28s_fDI). Make sure to use the latest recommended `main` branch rather than v10 that's mentioned in the video.

> **npm package**: `lib/` is also published to npm as [`@umbraco-cms/acceptance-test-helpers`](https://www.npmjs.com/package/@umbraco-cms/acceptance-test-helpers). See [README.npm.md](./README.npm.md) for the consumer-facing package documentation.
>
> Note the two different specifiers: **consumers** import from the published name `@umbraco-cms/acceptance-test-helpers`, while **tests inside this repository** import from `@umbraco/acceptance-test-helpers` — a path alias declared in [`tsconfig.json`](./tsconfig.json) that resolves straight to `lib/index.ts`. Keep using the alias in-repo; it is what all existing specs use.

---

## Prerequisites

- **Node.js 24** (see [`.nvmrc`](./.nvmrc))
- **A running installed Umbraco instance** on URL: [https://localhost:44339](https://localhost:44339) (default development port)
  - Install using `SqlServer`/`LocalDb` as the tests execute too fast for `SQLite` to handle

> **Your local instance is not configured the way CI's is.** The nightly pipeline builds a purpose-made instance and copies the composers from `tests/Umbraco.Tests.AcceptanceTest.UmbracoProject/` into it, which:
> - **suspends scheduled publishing** (`SuspendScheduledPublishingComposer`) — otherwise the background publishing job can fire in the middle of a test;
> - enables **SQL Server delayed durability** (`SqlServerDelayedDurabilityComposer`) — no fsync per commit, which matters a lot for a write-heavy suite.
>
> Running against `src/Umbraco.Web.UI` instead gets neither, and that project also sets `ModelsBuilder:ModelsMode` to `InMemoryAuto` — so models are regenerated on **every** content-type change, and this suite changes content types hundreds of times per run.
>
> Practical consequence: a flake you see only locally may be an artefact of your instance, not a bug in the test. Before chasing one, consider setting `ModelsMode` to `Nothing` locally and adding the composers above, so you are debugging the same conditions CI runs.

---

## Getting Started

1. Navigate to the test project folder:
   ```bash
   cd tests/Umbraco.Tests.AcceptanceTest
   ```

2. Install dependencies and Playwright browsers:
   ```bash
   npm ci
   npx playwright install
   ```

3. The setup script will prompt you to enter credentials for a superadmin user of your Umbraco CMS.

---

## Executing Tests

### Available NPM Scripts

| Command | Description |
|---------|-------------|
| `npm run test` | Execute DefaultConfig tests headlessly |
| `npm run ui` | Open Playwright UI mode with browser |
| `npm run smokeTest` | Run quick smoke tests (`@smoke` tagged) |
| `npm run smokeTestSqlite` | Smoke tests excluding User tests (SQLite limitation) |
| `npm run releaseTest` | Run comprehensive release tests (`@release` tagged) |
| `npm run richTextEditorTest` | Rich text editor — Tiptap, TinyMCE, the RTE data types and their rendering |
| `npm run blockEditorTest` | Block grid and block list, in both the data type and the content workspace |
| `npm run documentTypeTest` | Document types and element types, plus the content specs driven by their settings |
| `npm run dataTypeTest` | Every data type / property editor spec — the broadest of these, and the slowest |
| `npm run mediaTest` | Media, media types, the media pickers and media start nodes |
| `npm run memberTest` | Members, member groups, member types and the member pickers |
| `npm run userTest` | Users, user groups and permissions |
| `npm run languageTest` | Languages, cultures and variant content |
| `npm run templatingTest` | Templates, partial views, stylesheets and scripts |
| `npm run renderingTest` | Front-end rendering of content |
| `npm run all` | Run all test suites |
| `npm run testSqlite` | Run tests excluding User tests (SQLite limitation) |
| `npm run testWindows` | Run tests excluding RelationType tests |
| `npm run createTest <name> [projectDir]` | Generate a new test file template |
| `npm run config` | Reconfigure environment settings |
| `npm run build` | Compile `lib/` to `dist/` (does **not** type-check `tests/`) |
| `npm run typecheck` | Type-check `lib/` **and** `tests/` |

> Every `test`/`ui`/`smokeTest`/… script runs `npm run build` first, so `lib/` changes are picked up automatically.

> **The area scripts** (`richTextEditorTest` … `renderingTest`) exist so a change to one product area can be
> checked without running the whole suite. They overlap on purpose — a media picker spec is in both
> `mediaTest` and `dataTypeTest`, which is the point: each is an entry point from a different direction.
> To see what one covers before running it, append `--list` to the underlying command.

> They filter on the **spec file path**, not on a tag, so a new spec dropped into an area directory is picked
> up with no script change. Two things to know before adding one:
>
> - A positional filter is a **case-insensitive regex matched against the absolute path**. A bare `Users`
>   therefore matches every spec on a machine whose checkout lives under `C:\Users\…`, and a bare `Script`
>   matches `UserGroupsDescription.spec.ts`. That is why each fragment is written `"DefaultConfig.*Media"` —
>   the prefix anchors the match inside the test tree, and a leading separator (`"DefaultConfig.*/Script"`)
>   pins it to a directory when the word is a common substring.
> - Multiple fragments are OR-ed, and `/` works as the separator on Windows too.
>
> So add an area as one anchored fragment per directory or filename that identifies it, then confirm it with
> `npx playwright test "DefaultConfig.*YourArea" --list` before committing — that is the only thing that shows
> a fragment matching more than you meant.

### Before committing

```bash
npm run typecheck   # a type error in a spec will not surface from `npm run build`
```

It needs no running Umbraco instance, and it runs in CI (`build/azure-pipelines.yml`, `Build` stage, job C), so a type regression fails the build.

**There is no lint step.** The determinism conventions in [CLAUDE.md](./CLAUDE.md) §3 — awaiting every assertion, waiting on state rather than time, exact-name locators, tearing down global entities — are applied by reading and by review, not by a tool. A green `typecheck` proves the types line up and nothing more.

### Running Single Tests

Run a specific test file:
```bash
npx playwright test tests/DefaultConfig/Content/Content.spec.ts
```

Run a single test by name:
```bash
npx playwright test -g "can create content with the document link"
```

Run tests with visible browser:
```bash
npx playwright test --headed tests/DefaultConfig/Content/Content.spec.ts
```

### UI Mode

For an interactive testing experience with step-by-step visualization:

```bash
npx playwright test --ui
```

Or specify a test directory:
```bash
npx playwright test --ui tests/DefaultConfig
```

> **Note**: In UI mode, if you only see the authenticate test, click on 'Projects' and select 'defaultConfig' to see all tests.

---

## Writing Tests

### Test Structure (AAA Pattern)

All tests follow the Arrange-Act-Assert pattern:

```typescript
import { ConstantHelper, test } from '@umbraco/acceptance-test-helpers';
import { expect } from '@playwright/test';

const documentTypeName = 'TestDocumentType';
const contentName = 'TestContent';

test.beforeEach(async ({ umbracoApi, umbracoUi }) => {
  // Clean up any existing test data (idempotent)
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);

  // Navigate to backoffice
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({ umbracoApi }) => {
  // Always clean up after tests
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
});

test('can create content', { tag: '@smoke' }, async ({ umbracoApi, umbracoUi }) => {
  // Arrange - Setup test data via API
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

  // Act - Perform UI actions
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert - Verify results
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});
```

### Test Tags

Tag tests for selective execution:

```typescript
test('critical path test', { tag: '@smoke' }, async ({ umbracoApi, umbracoUi }) => {
  // Quick smoke test
});

test('comprehensive test', { tag: '@release' }, async ({ umbracoApi, umbracoUi }) => {
  // Full release validation
});
```

### Creating a New Test

Use the generator script:
```bash
npm run createTest MyFeatureName            # -> tests/DefaultConfig/MyFeatureName.spec.ts
npm run createTest MyFeatureName DeliveryApi # -> tests/DeliveryApi/MyFeatureName.spec.ts
```

It scaffolds a spec that already follows the conventions below — idempotent cleanup in `beforeEach`/`afterEach`, API setup, AAA body — and refuses to overwrite an existing file. The second argument picks the project directory; it defaults to `DefaultConfig` because a spec outside a project directory never runs.

### Test Conventions

1. **Idempotent cleanup**: Use `ensureNameNotExists()` instead of `delete()` — won't fail if the item doesn't exist
2. **API for setup**: Create test data via API (faster than UI)
3. **UI for validation**: Test actual user workflows through the UI
4. **Test independence**: Each test should run standalone without depending on other tests
5. **Clear the name before creating**: entity names are shared widely across specs (`TestContent` appears in dozens of them), and what keeps that safe is `workers: 1` plus `create*` helpers that call `ensureNameNotExists` first — not per-file uniqueness. If you add a `create*` helper, ensure the name first. See [CLAUDE.md](./CLAUDE.md) §4.
6. **Descriptive names**: Use clear, descriptive test and variable names
7. **Clean up**: Always clean up test data in `afterEach`

> **Determinism rules live in [CLAUDE.md](./CLAUDE.md) §3**, which opens with a 12-row checklist — read that before adding a spec or a helper, and follow a link into the detail when you hit that case. Most flaky failures trace back to one of those twelve.

---

## Environment Configuration

The environment configuration is set up by the npm installation script, creating a `.env` file (git-ignored):

```bash
UMBRACO_USER_LOGIN=email@example.com
UMBRACO_USER_PASSWORD=yourpassword
URL=https://localhost:44339
```

To reconfigure:
```bash
npm run config
```

---

## Debugging Tests

### Pause Execution

```typescript
await page.pause();  // Opens Playwright Inspector
```

### Take Screenshots

```typescript
await page.screenshot({ path: 'debug.png' });
```

### View Traces

Failed tests automatically save traces. View them with:
```bash
npx playwright show-trace results/trace.zip
```

### Run in Debug Mode

```bash
PWDEBUG=1 npx playwright test tests/DefaultConfig/Content/Content.spec.ts
```

---

## Test Projects

The test suite is organized into multiple Playwright projects (see `playwright.config.ts`):

| Project | Matches | Authenticated |
|---------|---------|---------------|
| `setup` | `**/*.setup.ts` | — (produces the auth state) |
| `defaultConfig` | `DefaultConfig/**` | yes |
| `extensionRegistry` | `ExtensionRegistry/**/*.spec.ts` | yes |
| `entityDataPicker` | `EntityDataPicker/**/*.spec.ts` | yes |
| `deliveryApi` | `DeliveryApi/**` | yes |
| `contentSettingConfig` | `ContentSettingConfig/**` | yes |
| `smtp` | `SMTP/*.spec.ts` | yes |
| `imagingSettingConfig` | `ImagingSettingConfig/*.spec.ts` | yes |
| `externalLoginAzureADB2C` | `ExternalLogin/AzureADB2C/**` | no |
| `authProviderLateRegistration` | `AuthProviderLateRegistration/**/*.spec.ts` | no (exercises the login screen) |
| `unattendedInstallConfig` | `UnattendedInstallConfig/**` | no (exercises install) |

Every authenticated project declares `dependencies: ['setup']` and reuses the stored `storageState`.

> **A spec must live under a directory one of these projects matches.** A file written straight into `tests/` matches no `testMatch` pattern and is silently never run — which is why `npm run createTest` writes into `tests/DefaultConfig/` by default (pass a second argument to target another project directory).

### Every project except `defaultConfig` needs its instance set up first

This is the part the Playwright config cannot tell you, and it is why these specs look unrunnable when
they are not: each of those projects ships a `tests/<Project>/AdditionalSetup/` folder whose contents have
to be installed into the Umbraco instance **before** the project will pass. Point a plain instance at
`--project=deliveryApi` and the tests run — they just fail, because the feature under test was never
switched on.

| Project | `AdditionalSetup` carries |
|---------|---------------------------|
| `extensionRegistry` | `appsettings.json`, nine `App_Plugins` bundles, **and `Segments/MySegmentService.cs`** |
| `entityDataPicker` | `appsettings.json` + the `picker-data-source` plugin |
| `deliveryApi` | `appsettings.json` + a replacement `Program.cs` |
| `authProviderLateRegistration` | the `LateAuthProvider` plugin (no appsettings) |
| `externalLoginAzureADB2C` | `appsettings.json`, a `Login` plugin, four `.cs` files |
| `contentSettingConfig`, `imagingSettingConfig`, `smtp`, `unattendedInstallConfig` | `appsettings.json` only |

`build/nightly-E2E-build-template.yml` is the reference implementation, and it is three copies into the
instance root: every `*.json` at the top level, any `App_Plugins` directory, and every `*.cs` **recursively,
preserving its relative path**. Reproduce those three by hand and the project passes.

Two traps when the instance is `src/Umbraco.Web.UI` rather than the pipeline-built project:

- **Do not copy `DeliveryApi/AdditionalSetup/Program.cs` over it.** That file has no
  `appsettings.Local.json` line, so it silently drops your connection string and unattended-install
  settings. Add `UseDeliveryApi` to `<DefineConstants>` in `Umbraco.Web.UI.csproj` instead — the
  `#if UseDeliveryApi` block is already there, and everything else keeps working.
- **A `.cs` file means a rebuild.** `extensionRegistry` reads as flaky without one: no `ISegmentService` is
  registered, so the variant selector never renders and every segment test fails on a locator that is
  perfectly correct.

> `appsettings.Local.json` is added inside `#if DEBUG` in `Program.cs`, so **a Release build ignores it
> entirely** — no connection string, no unattended install, and the instance sits at `serverStatus: Install`
> serving a login page with no fields. Run the local instance in Debug. Relatedly, unattended install will
> populate an empty SQL Server database but will not create one, so the database must exist first.

---

## Configuration Details

`playwright.config.ts` owns the timeouts, retries, worker count, trace mode, browser and
`testIdAttribute`. Read it there rather than here — a value written in two places disagrees
eventually, which is how this file once advertised a 30s timeout against a config saying 60s.

The two choices it cannot explain itself:

- **`workers: 1` is not a performance setting.** Specs share fixed entity names and would
  collide in parallel; see [CLAUDE.md](./CLAUDE.md) section 4.
- **`testIdAttribute` is `data-mark`**, so `getByTestId()` reads `data-mark`, not
  `data-testid`. Worth knowing before a locator mystifies you.

Locally, switching `trace` to `on-first-retry` roughly halves run time.

---

## Documentation

- [Playwright Documentation](https://playwright.dev/docs/intro)
- [Umbraco Documentation](https://docs.umbraco.com/)
- [`@umbraco-cms/acceptance-test-helpers`](https://www.npmjs.com/package/@umbraco-cms/acceptance-test-helpers) (published from this project)
- [CLAUDE.md](./CLAUDE.md) — architecture and the determinism conventions that keep the suite stable
