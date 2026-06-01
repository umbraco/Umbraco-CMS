import {ConstantHelper, test} from "@umbraco/acceptance-test-helpers";
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
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.variesByCulture).toBeTruthy();
});

// On V16 Segments will not be allowed through the UI, but the server.
test.skip('can add allow segmentation for a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickVaryBySegmentsButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
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
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();
});

test('can disable history cleanup for a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  // Is needed
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickPreventCleanupButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.cleanup.preventCleanup).toBeTruthy();
});

test('cannot see History Cleanup section in Settings tab for an Element Type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createEmptyElementType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for the UI to update after toggling Element Type

  // Assert
  await umbracoUi.documentType.isPreventCleanupButtonVisible(false);
  await umbracoUi.documentType.isElementTypeNotApplicableMessageForPropertyWithNameVisible('History clean up');
});

test('can see History Cleanup section in Settings tab after toggling off Element Type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createEmptyElementType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickTextButtonWithName('Element Type');
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  await umbracoUi.documentType.isPreventCleanupButtonVisible(true);
  await umbracoUi.documentType.doesElementTypeNotApplicableMessageExist(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeFalsy();
});

test('can see History Cleanup section in Settings tab after enabling Allow in Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createEmptyElementType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.documentType.clickAllowInLibraryButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.isPreventCleanupButtonVisible(true);
  await umbracoUi.documentType.doesElementTypeNotApplicableMessageExist(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();
});

test('cannot see element type not applicable message in Settings tab for a Document Type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();

  // Assert
  await umbracoUi.documentType.doesElementTypeNotApplicableMessageExist(false);
  await umbracoUi.documentType.isPreventCleanupButtonVisible(true);
});

test('cannot disable Element Type when element of that type exists', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementName = 'TestElement';
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(documentTypeName, true);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickTextButtonWithName('Element Type');
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.doesErrorNotificationHaveText(ConstantHelper.elementTypeChangeMessages.elementHasContent);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();

  // Clean
  await umbracoApi.element.ensureNameNotExists(elementName);
});

test('can disable Element Type after deleting the element of that type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementName = 'TestElement';
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(documentTypeName, true);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.delete(elementId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickTextButtonWithName('Element Type');
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeFalsy();
});

test('cannot enable Element Type when document of that type exists', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentName = 'TestDocument';
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickTextButtonWithName('Element Type');
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.doesErrorNotificationHaveText(ConstantHelper.elementTypeChangeMessages.documentHasContent);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeFalsy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(documentName);
});

test('cannot disable Element Type when used in a block editor configuration', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockListDataTypeName = 'TestBlockListReferencingElement';
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(documentTypeName);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, elementTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickTextButtonWithName('Element Type');
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.doesErrorNotificationHaveText(ConstantHelper.elementTypeChangeMessages.elementUsedInBlockEditor);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
});
