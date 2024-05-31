import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const documentTypeName = 'TestDocumentType';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can add allow vary by culture for a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickVaryByCultureButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.variesByCulture).toBeTruthy();
});

test('can add allow segmentation for a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickVaryBySegmentsButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.variesBySegment).toBeTruthy();
});

test('can set is an element type for a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickTextButtonWithName('Element Type');
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();
});

// TODO: Unskip. Currently The cleanup is not updated upon save
test.skip('can disable history cleanup for a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  // Is needed
  await umbracoUi.waitForTimeout(200);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickAutoCleanupButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.cleanup.preventCleanup).toBeTruthy();
});
