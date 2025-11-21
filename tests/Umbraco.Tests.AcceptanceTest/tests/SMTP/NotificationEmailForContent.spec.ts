import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content
let contentId = '';
const contentName = 'TestContent';
const contentText = 'This is test content text';
const childContentName = 'ChildContent';
// Document Type
let documentTypeId = '';
const documentTypeName = 'TestDocumentTypeForContent';
const childDocumentTypeName = 'ChildDocumentType';
// Data Type
const textStringDataTypeName = 'Textstring';

test.beforeEach(async ({umbracoApi}) => {
  // Delete all emails from smtp4dev
  await umbracoApi.smtp.deleteAllEmails();
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, textStringDataTypeName, textStringDataType.id);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, textStringDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  // Delete all emails from smtp4dev
  await umbracoApi.smtp.deleteAllEmails();
});

test('can set up notification for a content item', async ({umbracoUi, umbracoApi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.Delete', 'Umb.Document.Publish'];
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickNotificationsActionMenuOption();

  // Act
  await umbracoUi.content.clickDocumentNotificationOptionWithName(notificationActionIds[0]);
  await umbracoUi.content.clickDocumentNotificationOptionWithName(notificationActionIds[1]);
  await umbracoUi.content.clickSaveModalButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNotificationExist(contentId, notificationActionIds[0])).toBeTruthy();
  expect(await umbracoApi.document.doesNotificationExist(contentId, notificationActionIds[1])).toBeTruthy();
});

test('can see notification when content is published', async ({umbracoUi, umbracoApi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.Publish'];
  const actionName = 'Publish';
  await umbracoApi.document.updatetNotifications(contentId, notificationActionIds);
  expect(await umbracoApi.document.doesNotificationExist(contentId, notificationActionIds[0])).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickPublishActionMenuOption();
  await umbracoUi.content.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  const expectedSubject = `Notification about ${actionName} performed on ${contentName}`;
  const startTime = Date.now();
  while (Date.now() - startTime < 20000) {
    const email = await umbracoApi.smtp.findEmailBySubject(expectedSubject);
    if (email) {
      return;
    }
    await new Promise(resolve => setTimeout(resolve, 500));
  }
  expect(await umbracoApi.smtp.doesNotificationEmailWithSubjectExist(actionName, contentName)).toBeTruthy();
});

test('can see notification when content is updated', async ({umbracoUi, umbracoApi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.Update'];
  const actionName = 'Update';
  await umbracoApi.document.updatetNotifications(contentId, notificationActionIds);
  expect(await umbracoApi.document.doesNotificationExist(contentId, notificationActionIds[0])).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterTextstring(contentText);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  expect(await umbracoApi.smtp.doesNotificationEmailWithSubjectExist(actionName, contentName)).toBeTruthy();
});

test('can see notification when content is trashed', async ({umbracoUi, umbracoApi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.Delete'];
  const actionName = 'Delete';
  await umbracoApi.document.updatetNotifications(contentId, notificationActionIds);
  expect(await umbracoApi.document.doesNotificationExist(contentId, notificationActionIds[0])).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashActionMenuOption();
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.waitForContentToBeTrashed();
  expect(await umbracoApi.smtp.doesNotificationEmailWithSubjectExist(actionName, contentName)).toBeTruthy();
});

test('can see notification when child content is created', async ({umbracoUi, umbracoApi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.Create'];
  const actionName = 'Create';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const parentDocumentId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const parentId = await umbracoApi.document.createDefaultDocument(contentName, parentDocumentId) || '';
  await umbracoApi.document.updatetNotifications(parentId, notificationActionIds);
  expect(await umbracoApi.document.doesNotificationExist(parentId, notificationActionIds[0])).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(childDocumentTypeName);
  await umbracoUi.content.enterContentName(childContentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.waitForContentToBeCreated();
  expect(await umbracoApi.smtp.doesNotificationEmailWithSubjectExist(actionName, childContentName)).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(childContentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can see notification when content is restored', async ({umbracoUi, umbracoApi}) => {
  // Arrange
  const notificationActionIds = ['Umb.DocumentRecycleBin.Restore'];
  const actionName = 'Restore';
  await umbracoApi.document.updatetNotifications(contentId, notificationActionIds);
  expect(await umbracoApi.document.doesNotificationExist(contentId, notificationActionIds[0])).toBeTruthy();
  await umbracoApi.document.moveToRecycleBin(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
  // Act
  await umbracoUi.content.clickCaretButtonForName('Recycle Bin');
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickRestoreActionMenuOption();
  await umbracoUi.content.clickRestoreButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.smtp.doesNotificationEmailWithSubjectExist(actionName, contentName)).toBeTruthy();
});

test('can see notification when content is duplicated', async ({umbracoUi, umbracoApi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.Duplicate'];
  const actionName = 'Copy';
  await umbracoApi.document.updatetNotifications(contentId, notificationActionIds);
  expect(await umbracoApi.document.doesNotificationExist(contentId, notificationActionIds[0])).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickDuplicateToActionMenuOption();
  await umbracoUi.content.clickLabelWithName('Content');
  await umbracoUi.content.clickDuplicateButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.smtp.doesNotificationEmailWithSubjectExist(actionName, contentName)).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(contentName + ' (1)');
});


