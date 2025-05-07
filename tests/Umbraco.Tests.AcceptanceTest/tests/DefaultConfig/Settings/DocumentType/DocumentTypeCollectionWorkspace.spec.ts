import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
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
  // Arrange
  await umbracoUi.documentType.clickDocumentTypesMenu();

  // Act
  await umbracoUi.documentType.clickCreateActionWithOptionName('Document Type');
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Check if the created document type is displayed in the collection view and has correct icon
  await umbracoUi.documentType.clickDocumentTypesMenu();
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveName(documentTypeName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveIcon(documentTypeName, 'icon-document');
});

test('can create a document type with a template using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.template.ensureNameNotExists(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypesMenu();
  
  // Act
  await umbracoUi.documentType.clickCreateActionWithOptionName('Document Type with Template');
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  // Checks if both the success notification for document Types and the template are visible
  //await umbracoUi.documentType.doesSuccessNotificationsHaveCount(2);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  // Checks if the documentType contains the template
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const templateData = await umbracoApi.template.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates[0].id).toEqual(templateData.id);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Check if the created document type is displayed in the collection view and has correct icon
  await umbracoUi.documentType.clickDocumentTypesMenu();
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveName(documentTypeName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveIcon(documentTypeName, 'icon-document-html');

  // Clean
  await umbracoApi.template.ensureNameNotExists(documentTypeName);
});

test('can create a element type using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.documentType.clickDocumentTypesMenu();
  
  // Act
  await umbracoUi.documentType.clickCreateActionWithOptionName('Element Type');
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Checks if the isElement is true
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();
  // Check if the created document type is displayed in the collection view and has correct icon
  await umbracoUi.documentType.clickDocumentTypesMenu();
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveName(documentTypeName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveIcon(documentTypeName, 'icon-plugin');
});

test('can create a document type folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.documentType.clickDocumentTypesMenu();
  
  // Act
  await umbracoUi.documentType.clickCreateActionWithOptionName('Folder');
  await umbracoUi.documentType.enterFolderName(documentFolderName);
  await umbracoUi.documentType.clickCreateFolderButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const folder = await umbracoApi.documentType.getByName(documentFolderName);
  expect(folder.name).toBe(documentFolderName);
  // Check if the created document type folder is displayed in the collection view and has correct icon
  await umbracoUi.documentType.clickDocumentTypesMenu();
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveName(documentFolderName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveIcon(documentFolderName, 'icon-folder');
});

test('can create a document type in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createFolder(documentFolderName);
  await umbracoUi.documentType.goToDocumentType(documentFolderName);

  // Act
  await umbracoUi.documentType.clickCreateActionWithOptionName('Document Type');
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Check if the created document type is displayed in the collection view and has correct icon
  await umbracoUi.documentType.goToDocumentType(documentFolderName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveName(documentTypeName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveIcon(documentTypeName, 'icon-document');
});

test('can create a document type with a template in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.template.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.createFolder(documentFolderName);
  await umbracoUi.documentType.goToDocumentType(documentFolderName);
  
  // Act
  await umbracoUi.documentType.clickCreateActionWithOptionName('Document Type with Template');
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  // Checks if both the success notification for document Types and the template are visible
  //await umbracoUi.documentType.doesSuccessNotificationsHaveCount(2);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  // Checks if the documentType contains the template
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const templateData = await umbracoApi.template.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates[0].id).toEqual(templateData.id);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Check if the created document type is displayed in the collection view and has correct icon
  await umbracoUi.documentType.goToDocumentType(documentFolderName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveName(documentTypeName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveIcon(documentTypeName, 'icon-document-html');

  // Clean
  await umbracoApi.template.ensureNameNotExists(documentTypeName);
});

test('can create a element type in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createFolder(documentFolderName);
  await umbracoUi.documentType.goToDocumentType(documentFolderName);
  
  // Act
  await umbracoUi.documentType.clickCreateActionWithOptionName('Element Type');
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Checks if the isElement is true
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();
  // Check if the created document type is displayed in the collection view and has correct icon
  await umbracoUi.documentType.goToDocumentType(documentFolderName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveName(documentTypeName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveIcon(documentTypeName, 'icon-plugin');
});

test('can create a document type folder in a folder using create options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'Test Child Folder';
  await umbracoApi.documentType.ensureNameNotExists(childFolderName);
  await umbracoApi.documentType.createFolder(documentFolderName);
  await umbracoUi.documentType.goToDocumentType(documentFolderName);
  
  // Act
  await umbracoUi.documentType.clickCreateActionWithOptionName('Folder');
  await umbracoUi.documentType.enterFolderName(childFolderName);
  await umbracoUi.documentType.clickCreateFolderButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const folder = await umbracoApi.documentType.getByName(childFolderName);
  expect(folder.name).toBe(childFolderName);
  // Check if the created document type folder is displayed in the collection view and has correct icon
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveName(childFolderName);
  await umbracoUi.documentType.doesCollectionTreeItemTableRowHaveIcon(childFolderName, 'icon-folder');

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(childFolderName);
});
