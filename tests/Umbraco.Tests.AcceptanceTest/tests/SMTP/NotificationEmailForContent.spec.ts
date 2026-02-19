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
let textStringDataType: any;

test.beforeEach(async ({umbracoApi}) => {
  // Delete all emails from smtp4dev
  await umbracoApi.smtp.deleteAllEmails();
  textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
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
  await umbracoUi.content.clickSaveModalButtonAndWaitForNotificationToBeCreated();

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
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
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
  await umbracoUi.content.clickConfirmTrashButtonAndWaitForContentToBeTrashed();

  // Assert
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
  const childContentId = await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesExist(childContentId)).toBe(true);
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

test('can see notification when content is rollbacked', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.Rollback'];
  const actionName = 'Rollback';
  await umbracoApi.document.updatetNotifications(contentId, notificationActionIds);
  expect(await umbracoApi.document.doesNotificationExist(contentId, notificationActionIds[0])).toBeTruthy();
  await umbracoApi.document.publish(contentId);
  const updatedContentText = 'This is an updated content text';
  const contentData = await umbracoApi.document.get(contentId);
  contentData.values[0].value = updatedContentText;
  await umbracoApi.document.update(contentId, contentData);
  await umbracoApi.document.publish(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesDocumentPropertyHaveValue(textStringDataTypeName, updatedContentText);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium); // Wait for the rollback items to load
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.clickContentTab();
  await umbracoUi.content.doesDocumentPropertyHaveValue(textStringDataTypeName, contentText);
  expect(await umbracoApi.smtp.doesNotificationEmailWithSubjectExist(actionName, contentName)).toBeTruthy();
});

test('can see notification when content is sorted', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.Sort'];
  const actionName = 'Sort';
  // Create content with children to sort
  const rootDocumentTypeName = 'RootDocumentType';
  const childDocumentTypeName = 'ChildDocumentTypeOne';
  const rootDocumentName = 'RootDocument';
  const childDocumentOneName = 'FirstChildDocument';
  const childDocumentTwoName = 'SecondChildDocument';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const rootDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndDataType(rootDocumentTypeName, childDocumentTypeId, textStringDataTypeName, textStringDataType.id);
  const rootDocumentId = await umbracoApi.document.createDocumentWithTextContent(rootDocumentName, rootDocumentTypeId, contentText, textStringDataTypeName);
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentOneName, childDocumentTypeId, rootDocumentId);
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentTwoName, childDocumentTypeId, rootDocumentId);
  // Set notification on the root document
  await umbracoApi.document.updatetNotifications(rootDocumentId, notificationActionIds);
  expect(await umbracoApi.document.doesNotificationExist(rootDocumentId, notificationActionIds[0])).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickSortChildrenActionMenuOption();
  const firstDocumentLocator = umbracoUi.content.getTextLocatorWithName(childDocumentOneName);
  const secondDocumentLocator = umbracoUi.content.getTextLocatorWithName(childDocumentTwoName);
  await umbracoUi.content.dragAndDrop(secondDocumentLocator, firstDocumentLocator);
  await umbracoUi.content.clickSortButton();

  // Assert
  await umbracoUi.content.openContentCaretButtonForName(rootDocumentName);
  await umbracoUi.content.doesIndexDocumentInTreeContainName(rootDocumentName, childDocumentTwoName, 0);
  await umbracoUi.content.doesIndexDocumentInTreeContainName(rootDocumentName, childDocumentOneName, 1);
  expect(await umbracoApi.smtp.doesNotificationEmailWithSubjectExist(actionName, rootDocumentName)).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(rootDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can see notification when content is set public access', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const notificationActionIds = ['Umb.Document.PublicAccess'];
  const actionName = 'Restrict Public Access';
  await umbracoApi.document.updatetNotifications(contentId, notificationActionIds);
  expect(await umbracoApi.document.doesNotificationExist(contentId, notificationActionIds[0])).toBeTruthy();
  const testMemberGroup = 'TestMemberGroup';
  await umbracoApi.memberGroup.ensureNameNotExists(testMemberGroup);
  await umbracoApi.memberGroup.create(testMemberGroup);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickPublicAccessActionMenuOption();
  await umbracoUi.content.addGroupBasedPublicAccess(testMemberGroup, contentName);

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.smtp.doesNotificationEmailWithSubjectExist(actionName, contentName)).toBeTruthy();

  // Clean
  await umbracoApi.memberGroup.ensureNameNotExists(testMemberGroup);
});

