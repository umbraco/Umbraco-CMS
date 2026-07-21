import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const documentTypeName = 'TestDocumentType';
const documentFolderName = 'TestDocumentTypeFolder';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentFolderName);
});

test('can create a document type using create options', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.documentType.clickActionsMenuAtRoot();
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeCreated();

  // Assert
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Check the created document type is displayed in the tree
  await umbracoUi.documentType.reloadDocumentTypeTree()
  await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName);
});

test('can create a document type with a template using create options', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.template.ensureNameNotExists(documentTypeName);

  // Act
  await umbracoUi.documentType.clickActionsMenuAtRoot();
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentTypeWithTemplateButton();
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  const {documentTypeId, templateId} = await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeAndTemplateToBeCreated();

  // Assert
  // Checks if the documentType contains the template
  const documentTypeData = await umbracoApi.documentType.get(documentTypeId);
  expect(documentTypeData.allowedTemplates[0].id).toEqual(templateId);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();

  // Clean
  await umbracoApi.template.ensureNameNotExists(documentTypeName);
});

test('can create a element type using create options', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.documentType.clickActionsMenuAtRoot();
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateElementTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeCreated();

  // Assert
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Checks if the isElement is true
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();
  // Check the created element type is displayed in the tree
  await umbracoUi.documentType.reloadDocumentTypeTree();
  await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName);
});

test('can create a document type folder using create options', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.documentType.clickActionsMenuAtRoot();
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentFolderButton();
  await umbracoUi.documentType.enterFolderName(documentFolderName);
  await umbracoUi.documentType.clickCreateFolderButton();

  // Assert
  expect(await umbracoApi.documentType.doesNameExist(documentFolderName)).toBeTruthy();
  // Check the created folder is displayed in the tree
  await umbracoUi.documentType.reloadDocumentTypeTree();
  await umbracoUi.documentType.isDocumentTreeItemVisible(documentFolderName);
});

test('can create a document type in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentFolderId = await umbracoApi.documentType.createFolder(documentFolderName);
  await umbracoUi.documentType.reloadDocumentTypeTree();

  // Act
  await umbracoUi.documentType.clickActionsMenuForDocumentType(documentFolderName);
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeCreated();

  // Assert
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Verify the document type is inside the parent folder
  const parentFolderChildren = await umbracoApi.documentType.getChildren(parentFolderId);
  expect(parentFolderChildren[0].name).toBe(documentTypeName);
});

test('can create a document type with a template in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.template.ensureNameNotExists(documentTypeName);
  const parentFolderId = await umbracoApi.documentType.createFolder(documentFolderName);
  await umbracoUi.documentType.reloadDocumentTypeTree();

  // Act
  await umbracoUi.documentType.clickActionsMenuForDocumentType(documentFolderName);
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentTypeWithTemplateButton();
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  const {documentTypeId, templateId} = await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeAndTemplateToBeCreated();

  // Assert
  // Checks if the documentType contains the template
  const documentTypeData = await umbracoApi.documentType.get(documentTypeId);
  expect(documentTypeData.allowedTemplates[0].id).toEqual(templateId);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Verify the document type is inside the parent folder
  const parentFolderChildren = await umbracoApi.documentType.getChildren(parentFolderId);
  expect(parentFolderChildren[0].name).toBe(documentTypeName);

  // Clean
  await umbracoApi.template.ensureNameNotExists(documentTypeName);
});

test('can create a element type in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentFolderId = await umbracoApi.documentType.createFolder(documentFolderName);
  await umbracoUi.documentType.reloadDocumentTypeTree();

  // Act
  await umbracoUi.documentType.clickActionsMenuForDocumentType(documentFolderName);
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateElementTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeCreated();

  // Assert
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Checks if the isElement is true
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();
  // Verify the element type is inside the parent folder
  const parentFolderChildren = await umbracoApi.documentType.getChildren(parentFolderId);
  expect(parentFolderChildren[0].name).toBe(documentTypeName);
});

test('can create a document type folder in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'Test Child Folder';
  await umbracoApi.documentType.ensureNameNotExists(childFolderName);
  await umbracoApi.documentType.createFolder(documentFolderName);
  await umbracoUi.documentType.reloadDocumentTypeTree();

  // Act
  await umbracoUi.documentType.clickActionsMenuForDocumentType(documentFolderName);
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentFolderButton();
  await umbracoUi.documentType.enterFolderName(childFolderName);
  await umbracoUi.documentType.clickCreateFolderButton();

  // Assert
  const folder = await umbracoApi.documentType.getByName(childFolderName);
  expect(folder.name).toBe(childFolderName);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(childFolderName);
});
