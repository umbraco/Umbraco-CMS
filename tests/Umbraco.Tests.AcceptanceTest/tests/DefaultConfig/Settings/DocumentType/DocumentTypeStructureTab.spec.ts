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

test('can add allow as root to a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.documentType.clickAllowAtRootButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
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
  await umbracoUi.documentType.clickButtonWithName(documentTypeName);
  await umbracoUi.documentType.clickAllowedChildNodesButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
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
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedDocumentTypes.length).toBe(0);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can configure a collection for a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionDataTypeName = 'TestCollection';
  await umbracoApi.dataType.ensureNameNotExists(collectionDataTypeName);
  const collectionDataTypeId = await umbracoApi.dataType.create(collectionDataTypeName, 'Umbraco.ListView', 'Umb.PropertyEditorUi.Collection', []);
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.documentType.clickConfigureAsACollectionButton();
  await umbracoUi.documentType.clickTextButtonWithName(collectionDataTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.collection.id).toEqual(collectionDataTypeId);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(collectionDataTypeName);
});
