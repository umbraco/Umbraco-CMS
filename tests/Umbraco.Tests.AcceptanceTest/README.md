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
| `npm run all` | Run all test suites |
| `npm run testSqlite` | Run tests excluding User tests (SQLite limitation) |
| `npm run testWindows` | Run tests excluding RelationType tests |
| `npm run createTest <name> [projectDir]` | Generate a new test file template |
| `npm run config` | Reconfigure environment settings |
| `npm run build` | Compile `lib/` to `dist/` (does **not** type-check `tests/`) |
| `npm run typecheck` | Type-check `lib/` **and** `tests/` |
| `npm run audit` | Self-test the rules, then check the suite against CLAUDE.md §3 |
| `npm run audit:selftest` | Run just the audit's own rule tests |
| `npm run helpers:selftest` | Test the API assertion helpers (no Umbraco needed) |
| `npm run check` | typecheck + audit + helper tests |
| `npm run flaky` | Name the flaky and near-timeout tests from a run's JSON report |

> Every `test`/`ui`/`smokeTest`/… script runs `npm run build` first, so `lib/` changes are picked up automatically.

### Checks to run before committing

None of these needs a running Umbraco instance:

```bash
npm run check              # typecheck + audit + helper tests

npm run typecheck          # a type error in a spec won't surface from `npm run build`
npm run audit              # one line per convention; non-zero exit if a rule regresses
npm run audit -- --verbose # every finding, with file:line
npm run helpers:selftest   # the API assertion helpers, against fabricated responses
```

`npm run audit` self-tests every one of its rules before reporting — over half gate at budget 0, where a broken regex would otherwise be indistinguishable from a passing rule. Adding a rule without a test case fails the self-test.

There is no lint step. `npm run audit` is the closest thing — it enforces the mechanical parts of [CLAUDE.md](./CLAUDE.md) §3 (dropped promises, silently-discarded assertions, specs that no project runs, un-annotated skipped tests) at a budget of zero, and ratchets the known debt (fixed sleeps, force clicks, substring name locators, untyped builders) so it can shrink but not grow. See CLAUDE.md §7 for the budget table.

**All three run in CI** — `build/azure-pipelines.yml`, `Build` stage, job C — so a regression fails the build rather than waiting to be noticed.

### After a run: which tests were flaky

`retries: 2` means a test that fails twice and passes on the third attempt is reported **green**, and the JUnit report CI publishes keeps only the final result. So the flaky set has never been visible — known as folklore, never as a list. Every run now also writes a JSON report, which records each attempt separately:

```bash
npm run flaky                       # reads results/results.json from your last run
npm run flaky -- <path/to/report>   # or a results.json from a nightly artifact
```

It reports three groups: tests that only passed after a retry, tests that failed every attempt, and tests whose slowest attempt ran past half the 60s timeout — that last group being the ones that pass on an idle agent and fail on a loaded one. It only reports; it never fails a build.

CI publishes `results/` as the *"Acceptance Test Results"* pipeline artifact, so pointing this at a few downloaded nightlies is what turns a single run into a trend.

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
5. **Clear the name before creating**: entity names are shared widely across specs (`TestContent` appears in 79 files), and what keeps that safe is `workers: 1` plus `create*` helpers that call `ensureNameNotExists` first — not per-file uniqueness. If you add a `create*` helper, ensure the name first. See [CLAUDE.md](./CLAUDE.md) §4.
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

---

## Configuration Details

Key settings in `playwright.config.ts`:

- **Test timeout**: 60 seconds per test
- **Expect timeout**: 5 seconds for assertions
- **Retries**: 2 — everywhere, not just on CI
- **Workers**: 1 (sequential execution; specs share fixed entity names and would collide in parallel)
- **Trace**: `retain-on-failure` (switch to `on-first-retry` locally to roughly halve run time)
- **Browser**: Desktop Chrome with `ignoreHTTPSErrors`
- **Test identifier**: `data-mark` attribute (so `getByTestId()` reads `data-mark`)
- **`forbidOnly`**: enabled on CI — a stray `test.only` fails the build

---

## Documentation

- [Playwright Documentation](https://playwright.dev/docs/intro)
- [Umbraco Documentation](https://docs.umbraco.com/)
- [`@umbraco-cms/acceptance-test-helpers`](https://www.npmjs.com/package/@umbraco-cms/acceptance-test-helpers) (published from this project)
- [CLAUDE.md](./CLAUDE.md) — architecture and the determinism conventions that keep the suite stable
