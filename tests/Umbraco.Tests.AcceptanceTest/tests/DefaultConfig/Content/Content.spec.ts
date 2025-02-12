import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let documentTypeId = '';
let contentId = '';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Textstring';
const contentText = 'This is test content text';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create empty content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can save and publish empty content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can create content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.enterTextstring(contentText);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value).toBe(contentText);
});

test('can rename content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongContentName = 'Wrong Content Name';
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  contentId = await umbracoApi.document.createDefaultDocument(wrongContentName, documentTypeId);
  expect(await umbracoApi.document.doesNameExist(wrongContentName)).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(wrongContentName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const updatedContentData = await umbracoApi.document.get(contentId);
  expect(updatedContentData.variants[0].name).toEqual(contentName);
});

test('can update content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongContentText = 'This is wrong test content text';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, wrongContentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterTextstring(contentText);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const updatedContentData = await umbracoApi.document.get(contentId);
  expect(updatedContentData.variants[0].name).toEqual(contentName);
  expect(updatedContentData.values[0].value).toBe(contentText);
});

test('can publish content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
});

test('can unpublish content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickUnpublishButton();
  await umbracoUi.content.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Draft');
});
