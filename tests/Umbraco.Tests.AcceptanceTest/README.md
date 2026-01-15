# Umbraco Acceptance Tests

End-to-end acceptance tests for Umbraco CMS using [Playwright](https://playwright.dev/).

You can watch a video following these instructions [here](https://www.youtube.com/watch?v=N4hBKB0U-d8) and a longer UmbraCollab recording [here](https://www.youtube.com/watch?v=hvoI28s_fDI). Make sure to use the latest recommended `main` branch rather than v10 that's mentioned in the video.

---

## Prerequisites

- **Node.js 22+**
- **A running installed Umbraco instance** on URL: [https://localhost:44339](https://localhost:44339) (default development port)
  - Install using `SqlServer`/`LocalDb` as the tests execute too fast for `SQLite` to handle

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
| `npm run releaseTest` | Run comprehensive release tests (`@release` tagged) |
| `npm run all` | Run all test suites |
| `npm run testSqlite` | Run tests excluding User tests (SQLite limitation) |
| `npm run testWindows` | Run tests excluding RelationType tests |
| `npm run createTest <name>` | Generate a new test file template |
| `npm run config` | Reconfigure environment settings |

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

## Test Helpers and Fixtures

Tests use the `@umbraco/playwright-testhelpers` package which provides three main fixtures:

### `umbracoUi` - UI Interaction Helper

For browser interactions organized by section:

```typescript
// Navigation
await umbracoUi.goToBackOffice();
await umbracoUi.content.goToSection(ConstantHelper.sections.content);

// Interactions
await umbracoUi.content.enterContentName('My Content');
await umbracoUi.content.clickSaveButton();
await umbracoUi.content.clickSaveAndPublishButton();

// Assertions
await umbracoUi.content.isSuccessStateVisibleForSaveButton();
await umbracoUi.content.doesSuccessNotificationHaveText('Content saved');
```

### `umbracoApi` - REST API Helper

For server-side operations (setup/teardown):

```typescript
// Create test data
const docTypeId = await umbracoApi.documentType.createDefaultDocumentType('TestType');
const dataTypeId = await umbracoApi.dataType.createTextstringDataType('TestDataType');

// Query data
const exists = await umbracoApi.document.doesNameExist('MyContent');
const data = await umbracoApi.document.getByName('MyContent');

// Cleanup (idempotent - won't fail if not exists)
await umbracoApi.documentType.ensureNameNotExists('TestType');

// Publishing
await umbracoApi.document.publish(documentId);
```

### `page` - Raw Playwright Page

Direct access to Playwright's Page object for custom interactions:

```typescript
await page.pause();  // Pause for debugging
await page.screenshot({ path: 'debug.png' });
```

### Helper Constants

```typescript
import { ConstantHelper, NotificationConstantHelper, AliasHelper } from '@umbraco/playwright-testhelpers';

// Section names
ConstantHelper.sections.content
ConstantHelper.sections.media
ConstantHelper.sections.settings

// Notification messages
NotificationConstantHelper.success.published
NotificationConstantHelper.success.saved

// String utilities
AliasHelper.toAlias('Test Document Type')  // → 'testDocumentType'
```

---

## Writing Tests

### Test Structure (AAA Pattern)

All tests follow the Arrange-Act-Assert pattern:

```typescript
import { ConstantHelper, test } from '@umbraco/playwright-testhelpers';
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
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

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
npm run createTest MyFeatureName
```

This creates `tests/MyFeatureName.spec.ts` with a template.

### Test Conventions

1. **Idempotent cleanup**: Use `ensureNameNotExists()` instead of `delete()` - won't fail if item doesn't exist
2. **API for setup**: Create test data via API (faster than UI)
3. **UI for validation**: Test actual user workflows through the UI
4. **Test independence**: Each test should run standalone without depending on other tests
5. **Descriptive names**: Use clear, descriptive test and variable names
6. **Clean up**: Always clean up test data in `afterEach`

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

| Project | Description |
|---------|-------------|
| `setup` | Authentication setup (runs first) |
| `defaultConfig` | Main test suite (depends on setup) |
| `extensionRegistry` | Extension registry tests |
| `entityDataPicker` | Entity data picker tests |
| `deliveryApi` | Delivery API tests |
| `externalLoginAzureADB2C` | Azure AD B2C authentication tests |
| `unattendedInstallConfig` | Installation tests (no auth required) |
| `smtp` | Email/SMTP tests |

---

## Configuration Details

Key settings in `playwright.config.ts`:

- **Test timeout**: 30 seconds per test
- **Expect timeout**: 5 seconds for assertions
- **Retries**: 2 retries on CI (0 locally)
- **Workers**: 1 (sequential execution for state consistency)
- **Browser**: Desktop Chrome with HTTPS
- **Test identifier**: `data-mark` attribute

---

## Documentation

- [Playwright Documentation](https://playwright.dev/docs/intro)
- [Umbraco Documentation](https://docs.umbraco.com/)
- [@umbraco/playwright-testhelpers](https://www.npmjs.com/package/@umbraco/playwright-testhelpers)
- [@umbraco/json-models-builders](https://www.npmjs.com/package/@umbraco/json-models-builders)
