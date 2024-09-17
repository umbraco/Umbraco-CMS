import {ConstantHelper, test, AliasHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import exp = require("node:constants");

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'List View - Content Custom';
const childDocumentTypeName = 'ChildDocumentTypeForContent';
const childContentName = 'ChildContent';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can create content with the list view data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  const contentChildrenAmount = await umbracoApi.document.getChildren(contentData.id)
  expect(contentChildrenAmount).toEqual([]);
});

test('can publish content with the list view data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  const contentChildrenAmount = await umbracoApi.document.getChildren(contentData.id)
  expect(contentChildrenAmount).toEqual([]);
});

test('can create content with a child in the list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCreateContentWithName(childDocumentTypeName);
  await umbracoUi.content.enterNameInContainer(childContentName);
  await umbracoUi.content.clickSaveModalButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  const contentChildrenAmount = await umbracoApi.document.getChildrenAmount(contentData.id)
  expect(contentChildrenAmount).toEqual(1);
});

test('can publish content with a child in the list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.goToContentInListViewWithName(childContentName);
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  const contentChildrenAmount = await umbracoApi.document.getChildrenAmount(documentId)
  expect(contentChildrenAmount).toEqual(1);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe(expectedState);
});

test('can not publish child in a list when parent is not published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.goToContentInListViewWithName(childContentName);
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  // Content created, but not published
  await umbracoUi.content.doesSuccessNotificationsHaveCount(1);
  await umbracoUi.content.isErrorNotificationVisible();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  const contentChildrenAmount = await umbracoApi.document.getChildrenAmount(documentId)
  expect(contentChildrenAmount).toEqual(1);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe(expectedState);
});

test('child is removed from list after child content is deleted', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  const childAmountBeforeDelete = await umbracoApi.document.getChildrenAmount(documentId);
  expect(childAmountBeforeDelete).toEqual(1);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoApi.document.ensureNameNotExists(childContentName);
  await umbracoUi.content.doesListViewHaveNoItemsInList();

  // Assert
  const childAmountAfterDelete = await umbracoApi.document.getChildrenAmount(documentId);
  expect(childAmountAfterDelete).toEqual(0);
  expect(await umbracoApi.document.doesNameExist(childContentName)).toBeFalsy();
});

test('can sort list by name', async ({page, umbracoApi, umbracoUi}) => {
// Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const secondChildContentName = 'ASecondChildContent';
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildContentName, childDocumentTypeId, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  const childAmountBeforeDelete = await umbracoApi.document.getChildrenAmount(documentId);
  expect(childAmountBeforeDelete).toEqual(2);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickNameButtonInListView();

  // Assert
  await umbracoUi.content.doesFirstItemInListViewHaveName(secondChildContentName);
});

test('can publish child content from list', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  const publishData = {"publishSchedules": [{"culture": null}]};
  await umbracoApi.document.publish(documentId, publishData);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.selectContentWithNameInListView(childContentName);
  await umbracoUi.content.clickPublishSelectedListItems();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe(expectedState);
});

test('can not publish child content from list when parent is not published', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.selectContentWithNameInListView(childContentName);
  await umbracoUi.content.clickPublishSelectedListItems();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible();
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe(expectedState);
});

test('can unpublish child content from list', async ({page, umbracoApi, umbracoUi}) => {

});

test('can duplicate child content in list', async ({page, umbracoApi, umbracoUi}) => {

});

test('can move child content in list', async ({page, umbracoApi, umbracoUi}) => {

});

test('can trash child content in list', async ({page, umbracoApi, umbracoUi}) => {

});
