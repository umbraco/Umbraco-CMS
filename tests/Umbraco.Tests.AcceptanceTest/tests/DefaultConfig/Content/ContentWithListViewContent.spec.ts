import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'List View - Content Custom';
const childDocumentTypeName = 'ChildDocumentTypeForContent';
const childContentName = 'ChildContent';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

// Remove .fixme when the issue is fixed: https://github.com/umbraco/Umbraco-CMS/issues/18615
test.fixme('can create content with the list view data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const defaultListViewDataTypeName = 'List View - Content';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(defaultListViewDataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, defaultListViewDataTypeName, dataTypeData.id, childDocumentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(await umbracoApi.document.getChildrenAmount(contentData.id)).toEqual(0);
});

test('can publish content with the list view data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(0);
});

test('can create content with a child in the list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickCreateContentWithName(childDocumentTypeName);
  await umbracoUi.content.enterNameInContainer(childContentName);
  await umbracoUi.content.clickSaveModalButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(1);
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
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  // Currently necessary
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  await umbracoUi.content.goToContentInListViewWithName(childContentName);
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(1);
  // Checks if child is published
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
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.goToContentInListViewWithName(childContentName);
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  // Content created, but not published
  await umbracoUi.content.doesSuccessNotificationsHaveCount(1);
  await umbracoUi.content.isErrorNotificationVisible();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(1);
  // Checks if child is still in draft
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe(expectedState);
});

test('child is removed from list after child content is deleted', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(1);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.clickActionsMenuForContent(childContentName);
  await umbracoUi.content.clickTrashButton();
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesContentListHaveNoItemsInList();
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(0);
  expect(await umbracoApi.document.doesNameExist(childContentName)).toBeFalsy();
});

test('can sort list by name', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const secondChildContentName = 'ASecondChildContent';
  await umbracoApi.dataType.createListViewContentDataType(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildContentName, childDocumentTypeId, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  const childAmountBeforeDelete = await umbracoApi.document.getChildrenAmount(documentId);
  expect(childAmountBeforeDelete).toEqual(2);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickNameButtonInListView();

  // Assert
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(2);
  await umbracoUi.content.doesFirstItemInListViewHaveName(secondChildContentName);
});

test('can publish child content from list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoApi.document.publish(documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentWithNameInListView(childContentName);
  await umbracoUi.content.clickPublishSelectedListItems();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(1);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe(expectedState);
});

test('can not publish child content from list when parent is not published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentWithNameInListView(childContentName);
  await umbracoUi.content.clickPublishSelectedListItems();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible();
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe(expectedState);
});

test('can unpublish child content from list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const childDocumentId = await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoApi.document.publish(documentId);
  await umbracoApi.document.publish(childDocumentId);
  const childContentDataBeforeUnpublished = await umbracoApi.document.getByName(childContentName);
  expect(childContentDataBeforeUnpublished.variants[0].state).toBe('Published');
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentWithNameInListView(childContentName);
  await umbracoUi.content.clickUnpublishSelectedListItems();
  await umbracoUi.content.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe(expectedState);
});

test('can duplicate child content in list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDocumentName = 'SecondDocument';
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const secondDocumentId = await umbracoApi.document.createDefaultDocument(secondDocumentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentWithNameInListView(childContentName);
  await umbracoUi.content.clickDuplicateToSelectedListItems();
  await umbracoUi.content.selectDocumentWithNameAtRoot(secondDocumentName);

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.doesFirstItemInListViewHaveName(childContentName);
  await umbracoUi.content.goToContentWithName(secondDocumentName);
  await umbracoUi.content.doesFirstItemInListViewHaveName(childContentName);
  // Checks firstContentNode
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(1);
  // Checks secondContentNode
  expect(await umbracoApi.document.getChildrenAmount(secondDocumentId)).toEqual(1);
});

test('can move child content in list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDocumentName = 'SecondDocument';
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const secondDocumentId = await umbracoApi.document.createDefaultDocument(secondDocumentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentWithNameInListView(childContentName);
  await umbracoUi.content.clickMoveToSelectedListItems();
  await umbracoUi.content.selectDocumentWithNameAtRoot(secondDocumentName);

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.doesListViewContainCount(0);
  await umbracoUi.content.goToContentWithName(secondDocumentName);
  await umbracoUi.content.doesFirstItemInListViewHaveName(childContentName);
  // Checks firstContentNode
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(0);
  // Checks secondContentNode
  expect(await umbracoApi.document.getChildrenAmount(secondDocumentId)).toEqual(1);
});

test('can trash child content in list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentWithNameInListView(childContentName);
  await umbracoUi.content.clickTrashSelectedListItems();
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.doesListViewContainCount(0);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(0);
  await umbracoUi.content.isItemVisibleInRecycleBin(childContentName);
});

test('can search for child content in list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondChildName = 'SecondChildDocument';
  await umbracoApi.document.ensureNameNotExists(secondChildName);
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildName, childDocumentTypeId, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesListViewContainCount(2);

  // Act
  await umbracoUi.content.searchByKeywordInCollection(childContentName);

  // Assert
  await umbracoUi.content.doesListViewContainCount(1);
  await umbracoUi.content.doesFirstItemInListViewHaveName(childContentName);
});

test('can change from list view to grid view in list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.dataType.createListViewContentDataTypeWithAllPermissions(dataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAPropertyEditorAndAnAllowedChildNode(documentTypeName, dataTypeName, dataTypeData.id, childDocumentTypeId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isDocumentListViewVisible();

  // Act
  await umbracoUi.content.changeToGridView();

  // Assert
  await umbracoUi.content.isDocumentGridViewVisible();
});
