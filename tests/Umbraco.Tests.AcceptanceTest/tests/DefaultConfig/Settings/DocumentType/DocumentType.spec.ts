import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const documentTypeName = 'TestDocumentType';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.clickActionsMenuAtRoot();
  await umbracoUi.documentType.clickCreateButton();
  await umbracoUi.documentType.clickCreateDocumentTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  await umbracoUi.documentType.reloadTree('Document Types');
  await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName);
});

test('can create a document type with a template', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
  await umbracoApi.template.ensureNameNotExists(documentTypeName);

  // Act
  await umbracoUi.documentType.clickActionsMenuAtRoot();
  await umbracoUi.documentType.clickCreateButton();
  await umbracoUi.documentType.clickCreateDocumentTypeWithTemplateButton();
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

  // Clean
  await umbracoApi.template.ensureNameNotExists(documentTypeName);
});

test('can create a element type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.clickActionsMenuAtRoot();
  await umbracoUi.documentType.clickCreateButton();
  await umbracoUi.documentType.clickCreateElementTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Checks if the isElement is true
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();
});

test('can rename a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'NotADocumentTypeName';
  await umbracoApi.documentType.ensureNameNotExists(wrongName);
  await umbracoApi.documentType.createDefaultDocumentType(wrongName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(wrongName);
  await umbracoUi.waitForTimeout(1000);
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.waitForTimeout(1000);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  await umbracoUi.documentType.isDocumentTreeItemVisible(wrongName, false);
  await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName);
});

test('can update the alias for a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const oldAlias = AliasHelper.toAlias(documentTypeName);
  const newAlias = 'newDocumentTypeAlias';
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  const documentTypeDataOld = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeDataOld.alias).toBe(oldAlias);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.enterAliasName(newAlias);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName, true);
  const documentTypeDataNew = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeDataNew.alias).toBe(newAlias);
});

test('can add an icon for a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const bugIcon = 'icon-bug';
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.documentType.updateIcon(bugIcon);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.icon).toBe(bugIcon);
  await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName, true);
});

test('can delete a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();

  // Act
  await umbracoUi.documentType.clickRootFolderCaretButton();
  await umbracoUi.documentType.clickActionsMenuForDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDeleteAndConfirmButton();

  // Assert
  //await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeFalsy();
});
