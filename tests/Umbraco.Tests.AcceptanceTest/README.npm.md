# @umbraco/acceptance-test-helpers

Test helpers and builders for writing [Playwright](https://playwright.dev/) end-to-end tests for Umbraco CMS solutions.

This package provides API helpers, UI helpers, and JSON model builders to simplify acceptance testing of Umbraco backoffice functionality.

## Installation

```bash
npm install -D @umbraco/acceptance-test-helpers
```

## Configuration

The test helpers need to know how to connect and authenticate with your Umbraco instance. Set these environment variables before running your tests:

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `URL` | No | `https://localhost:44339` | Base URL of the Umbraco instance |
| `UMBRACO_USER_LOGIN` | Yes | — | Backoffice superadmin email |
| `UMBRACO_USER_PASSWORD` | Yes | — | Backoffice superadmin password |
| `STORAGE_STATE_PATH` | Recommended | — | Path to Playwright auth storage state JSON (should match `storageState` in your `playwright.config`). The API helpers read tokens from this file for REST calls. Without it, tokens are extracted from the live page context, which is slower and less reliable. |
| `CONSOLE_ERRORS_PATH` | No | — | Path to a JSON file where browser console errors are collected during test runs. |

You can set them via a `.env` file in your project root (loaded with [dotenv](https://www.npmjs.com/package/dotenv) or similar):

```env
URL=https://localhost:44339
UMBRACO_USER_LOGIN=admin@example.com
UMBRACO_USER_PASSWORD=your-password
STORAGE_STATE_PATH=./playwright/.auth/user.json
CONSOLE_ERRORS_PATH=./console-errors.json
```

Or pass them directly in CI:

```yaml
env:
  URL: https://localhost:44339
  UMBRACO_USER_LOGIN: $(TestUserEmail)
  UMBRACO_USER_PASSWORD: $(TestUserPassword)
```

## Usage

The package exports a custom Playwright `test` fixture that provides two main helper categories:

```typescript
import { ConstantHelper, test } from '@umbraco/acceptance-test-helpers';
import { expect } from '@playwright/test';

test('can create content', async ({ umbracoApi, umbracoUi }) => {
  // Setup via API
  await umbracoApi.documentType.createDefaultDocumentType('TestType');

  // UI interactions
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType('TestType');
  await umbracoUi.content.enterContentName('TestContent');
  await umbracoUi.content.clickSaveButton();

  // Assertions
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  expect(await umbracoApi.document.doesNameExist('TestContent')).toBeTruthy();
});
```

## Fixtures

The package extends Playwright's `test` with two fixtures plus the standard `page`:

### `umbracoApi` — REST API Helper

For server-side setup, teardown, and assertions:

```typescript
// Create test data
const docTypeId = await umbracoApi.documentType.createDefaultDocumentType('TestType');
const dataTypeId = await umbracoApi.dataType.createTextstringDataType('TestDataType');

// Query data
const exists = await umbracoApi.document.doesNameExist('MyContent');

// Cleanup (idempotent — won't fail if not exists)
await umbracoApi.documentType.ensureNameNotExists('TestType');

// Publishing
await umbracoApi.document.publish(documentId);
```

**Available API helpers**: `dataType`, `dictionary`, `document`, `documentBlueprint`, `documentType`, `healthCheck`, `indexer`, `language`, `login`, `logViewer`, `media`, `mediaType`, `member`, `memberGroup`, `memberType`, `modelsBuilder`, `objectTypes`, `package`, `partialView`, `publishedCache`, `redirectManagement`, `relationType`, `report`, `script`, `smtp`, `stylesheet`, `telemetry`, `template`, `temporaryFile`, `user`, `userGroup`, `webhook`, `contentDeliveryApi`, `mediaDeliveryApi`

### `umbracoUi` — UI Interaction Helper

For browser interactions organized by section:

```typescript
// Navigation
await umbracoUi.goToBackOffice();
await umbracoUi.content.goToSection(ConstantHelper.sections.content);

// Interactions
await umbracoUi.content.enterContentName('My Content');
await umbracoUi.content.clickSaveButton();

// Assertions
await umbracoUi.content.isSuccessStateVisibleForSaveButton();
await umbracoUi.content.doesSuccessNotificationHaveText('Content saved');
```

**Available UI helpers**: `content`, `contentRender`, `currentUserProfile`, `dataType`, `dictionary`, `documentBlueprint`, `documentType`, `examineManagement`, `externalLogin`, `form`, `healthCheck`, `install`, `language`, `login`, `logViewer`, `media`, `mediaType`, `member`, `memberGroup`, `memberType`, `modelsBuilder`, `package`, `partialView`, `profiling`, `publishedStatus`, `redirectManagement`, `relationType`, `script`, `stylesheet`, `telemetryData`, `template`, `user`, `userGroup`, `webhook`, `welcomeDashboard`

### `page` — Raw Playwright Page

Direct access to Playwright's Page object for custom interactions.

## Helper Constants

```typescript
import { ConstantHelper, NotificationConstantHelper, AliasHelper } from '@umbraco/acceptance-test-helpers';

// Section names
ConstantHelper.sections.content
ConstantHelper.sections.media
ConstantHelper.sections.settings

// Notification messages
NotificationConstantHelper.success.published
NotificationConstantHelper.success.saved

// String utilities
AliasHelper.toAlias('Test Document Type')  // -> 'testDocumentType'
```

## Builders

Build complex Umbraco JSON models for test setup using the builder pattern with fluent API and sensible defaults.

### Builder Conventions

| Method | Purpose |
|--------|---------|
| `with*()` | Set a property value |
| `add*()` | Create and add a child builder |
| `build()` | Construct the final JSON object |
| `done()` | Return to parent builder |

### Quick Start

```typescript
import { DocumentTypeBuilder, AliasHelper } from '@umbraco/acceptance-test-helpers';

// Minimal — uses defaults
const simpleDocType = new DocumentTypeBuilder().build();

// Custom configuration
const documentType = new DocumentTypeBuilder()
  .withName('Blog Post')
  .withAlias(AliasHelper.toAlias('Blog Post'))
  .withAllowedAsRoot(true)
  .withIcon('icon-newspaper')
  .addProperty()
    .withName('Title')
    .withDataTypeId(textStringDataTypeId)
    .done()
  .build();
```

### Available Builders

**Document Types**: `DocumentTypeBuilder`, `DocumentTypePropertyBuilder`, `DocumentTypeContainerBuilder`, `DocumentTypeCompositionBuilder`, `DocumentTypeAllowedDocumentTypeBuilder`, `DocumentTypeAllowedTemplateBuilder`

**Data Types** (34+ builders):
- **Text**: `TextStringDataTypeBuilder`, `TextAreaDataTypeBuilder`, `MultipleTextStringDataTypeBuilder`
- **Numbers**: `NumericDataTypeBuilder`, `DecimalDataTypeBuilder`, `SliderDataTypeBuilder`
- **Boolean**: `TrueFalseDataTypeBuilder`
- **Date/Time**: `DatePickerDataTypeBuilder`, `DateOnlyPickerDataTypeBuilder`, `TimeOnlyPickerDataTypeBuilder`, `DateTimePickerDataTypeBuilder`, `DateTimeWithTimeZonePickerDataTypeBuilder`
- **Selection**: `DropdownDataTypeBuilder`, `CheckboxListDataTypeBuilder`, `RadioboxDataTypeBuilder`, `TagsDataTypeBuilder`
- **Pickers**: `ContentPickerDataTypeBuilder`, `MultiNodeTreePickerDataTypeBuilder`, `MediaPickerDataTypeBuilder`, `MultiUrlPickerDataTypeBuilder`, `EntityDataPickerDataTypeBuilder`
- **Rich Content**: `TinyMCEDataTypeBuilder`, `TiptapDataTypeBuilder`, `MarkdownEditorDataTypeBuilder`, `CodeEditorDataTypeBuilder`
- **Complex**: `BlockListDataTypeBuilder`, `BlockGridDataTypeBuilder`, `ImageCropperDataTypeBuilder`, `ListViewDataTypeBuilder`
- **Other**: `LabelDataTypeBuilder`, `EmailAddressDataTypeBuilder`, `ApprovedColorDataTypeBuilder`, `UploadFieldDataTypeBuilder`

**Documents**: `DocumentBuilder`, `DocumentValueBuilder`, `DocumentVariantBuilder`, `DocumentDomainBuilder`

**Document Blueprints**: `DocumentBlueprintsBuilder`, `DocumentBlueprintsValueBuilder`, `DocumentBlueprintsVariantBuilder`

**Media**: `MediaTypeBuilder`, `MediaTypePropertyBuilder`, `MediaTypeContainerBuilder`, `MediaTypeCompositionBuilder`, `MediaTypeAllowedMediaTypeBuilder`, `MediaBuilder`, `MediaValueBuilder`, `MediaVariantBuilder`, `MediaValueDataBuilder`

**Members**: `MemberTypeBuilder`, `MemberTypePropertyBuilder`, `MemberTypeContainerBuilder`, `MemberTypeCompositionBuilder`, `MemberBuilder`, `MemberValueBuilder`, `MemberVariantBuilder`

**Users**: `UserBuilder`, `UserGroupBuilder`, `UserGroupPermissionBuilder`, `UserGroupDocumentPermissionBuilder`, `UserGroupPropertyValuePermissionBuilder`

**Other**: `WebhookBuilder`, `PackageBuilder`

## Documentation

- [Playwright Documentation](https://playwright.dev/docs/intro)
- [Umbraco Documentation](https://docs.umbraco.com/)
- [Source & Contributing](https://github.com/umbraco/Umbraco-CMS/tree/main/tests/Umbraco.Tests.AcceptanceTest)
