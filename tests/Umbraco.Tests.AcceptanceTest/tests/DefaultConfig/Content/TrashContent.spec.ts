import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let dataTypeId = '';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Textstring';
const contentText = 'This is test content text';

test.beforeEach(async ({umbracoApi}) => {
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataTypeData.id;
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.emptyRecycleBin();
});

test('can trash an invariant content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // TODO: verify the references list not displayed
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesDocumentItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('can trash a variant content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // TODO: verify the references list not displayed
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesDocumentItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('can trash a published content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // TODO: verify the references list not displayed
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesDocumentItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('can trash an invariant content that references one item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // verify the references list
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesDocumentItemExistInRecycleBin(contentName)).toBeTruthy();
});

test.fixme('can trash a variant content that references one item', async ({umbracoApi, umbracoUi}) => {
});

test.fixme('can trash an invariant content that references more than 3 items', async ({umbracoApi, umbracoUi}) => {
});

test.fixme('can trash a variant content that references more than 3 items', async ({umbracoApi, umbracoUi}) => {
});

test.fixme('can trash a multiple culture content that references one item', async ({umbracoApi, umbracoUi}) => {
});