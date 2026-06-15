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

test('can add allow as root to a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.documentType.clickAllowAtRootButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedAsRoot).toBeTruthy();
});

test('can add an allowed child node to a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.documentType.clickChooseButton();
  await umbracoUi.documentType.clickModalMenuItemWithName(documentTypeName);
  await umbracoUi.documentType.clickAllowedChildNodesButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedDocumentTypes[0].documentType.id).toBe(documentTypeData.id);
});

test('can remove an allowed child node from a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeName = 'ChildDocumentType';
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.documentType.clickRemoveButtonForName(childDocumentTypeName);
  await umbracoUi.documentType.clickConfirmRemoveButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedDocumentTypes.length).toBe(0);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can configure a collection for a document type', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionDataTypeName = 'TestCollection';
  await umbracoApi.dataType.ensureNameNotExists(collectionDataTypeName);
  const collectionDataTypeId = await umbracoApi.dataType.create(collectionDataTypeName, 'Umbraco.ListView', 'Umb.PropertyEditorUi.Collection', []);
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.documentType.clickAddCollectionButton();
  await umbracoUi.documentType.clickTextButtonWithName(collectionDataTypeName);
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.collection.id).toEqual(collectionDataTypeId);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(collectionDataTypeName);
});

test('cannot see Allow at Root toggle in Structure tab for an Element Type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createEmptyElementType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for loading to complete

  // Assert
  await umbracoUi.documentType.isAllowAtRootButtonVisible(false);
  await umbracoUi.documentType.isElementTypeNotApplicableMessageForPropertyWithNameVisible('Allow at root');
});

test('cannot see Allowed Child Nodes section in Structure tab for an Element Type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createEmptyElementType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for loading to complete
  // Assert
  await umbracoUi.documentType.isAllowedChildNodesButtonVisible(false);
  await umbracoUi.documentType.isElementTypeNotApplicableMessageForPropertyWithNameVisible('Allowed child node types');
});

test('cannot see Collection section in Structure tab for an Element Type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createEmptyElementType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for loading to complete

  // Assert
  await umbracoUi.documentType.isAddCollectionButtonVisible(false);
  await umbracoUi.documentType.isElementTypeNotApplicableMessageForPropertyWithNameVisible('Collection');
});

test('can see Allow at Root toggle in Structure tab after toggling off Element Type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createEmptyElementType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickTextButtonWithName('Element Type');
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();
  await umbracoUi.documentType.clickStructureTab();

  // Assert
  await umbracoUi.documentType.isAllowAtRootButtonVisible(true);
  await umbracoUi.documentType.isAllowedChildNodesButtonVisible(true);
  await umbracoUi.documentType.isAddCollectionButtonVisible(true);
  await umbracoUi.documentType.doesElementTypeNotApplicableMessageExist(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeFalsy();
});

test('cannot see element type not applicable message in Structure tab for a Document Type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for loading to complete

  // Assert
  await umbracoUi.documentType.doesElementTypeNotApplicableMessageExist(false);
  await umbracoUi.documentType.isAllowAtRootButtonVisible();
  await umbracoUi.documentType.isAllowedChildNodesButtonVisible();
  await umbracoUi.documentType.isAddCollectionButtonVisible();
});
