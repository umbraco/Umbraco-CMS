import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestContent';
const secondContentName = 'SecondContent';
const firstChildContentName = 'FirstChildContent';
const secondChildContentName = 'SecondChildContent';
let documentId = '';
let firstChildId = '';
let secondChildId = '';
// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
const childDocumentTypeName = 'ChildDocumentTypeForContent';
let documentTypeId = '';
// Data Type
const dataTypeName = 'List View - Content';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, dataTypeData.id);
  documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  firstChildId = await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, documentId);
  secondChildId = await umbracoApi.document.createDefaultDocumentWithParent(secondChildContentName, childDocumentTypeId, documentId);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.document.emptyRecycleBin();
});

test('can bulk publish multiple child content items from list view', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  await umbracoApi.document.publish(documentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentCardWithName(firstChildContentName);
  await umbracoUi.content.selectContentCardWithName(secondChildContentName);
  await umbracoUi.content.clickPublishSelectedListItems();
  await umbracoUi.content.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(2);
  const firstChildData = await umbracoApi.document.getByName(firstChildContentName);
  expect(firstChildData.variants[0].state).toBe(expectedState);
  const secondChildData = await umbracoApi.document.getByName(secondChildContentName);
  expect(secondChildData.variants[0].state).toBe(expectedState);
});

test('can bulk unpublish multiple child content items from list view', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  await umbracoApi.document.publish(documentId);
  await umbracoApi.document.publish(firstChildId);
  await umbracoApi.document.publish(secondChildId);
  const firstChildBeforeUnpublish = await umbracoApi.document.getByName(firstChildContentName);
  expect(firstChildBeforeUnpublish.variants[0].state).toBe('Published');
  const secondChildBeforeUnpublish = await umbracoApi.document.getByName(secondChildContentName);
  expect(secondChildBeforeUnpublish.variants[0].state).toBe('Published');
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentCardWithName(firstChildContentName);
  await umbracoUi.content.selectContentCardWithName(secondChildContentName);
  await umbracoUi.content.clickUnpublishSelectedListItems();
  await umbracoUi.content.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  const firstChildData = await umbracoApi.document.getByName(firstChildContentName);
  expect(firstChildData.variants[0].state).toBe(expectedState);
  const secondChildData = await umbracoApi.document.getByName(secondChildContentName);
  expect(secondChildData.variants[0].state).toBe(expectedState);
});

test('can bulk duplicate multiple child content items in list view', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDocumentId = await umbracoApi.document.createDefaultDocument(secondContentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentCardWithName(firstChildContentName);
  await umbracoUi.content.selectContentCardWithName(secondChildContentName);
  await umbracoUi.content.clickDuplicateToSelectedListItems();
  await umbracoUi.content.openCaretButtonForName('Content');
  await umbracoUi.content.clickModalMenuItemWithName(secondContentName);
  await umbracoUi.content.clickChooseModalButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(2);
  expect(await umbracoApi.document.getChildrenAmount(secondDocumentId)).toEqual(2);
});

test('can bulk move multiple child content items in list view', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDocumentId = await umbracoApi.document.createDefaultDocument(secondContentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentCardWithName(firstChildContentName);
  await umbracoUi.content.selectContentCardWithName(secondChildContentName);
  await umbracoUi.content.clickMoveToSelectedListItems();
  await umbracoUi.content.openCaretButtonForName('Content');
  await umbracoUi.content.clickModalMenuItemWithName(secondContentName);
  await umbracoUi.content.clickChooseModalButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  await umbracoUi.content.doesListViewContainCount(0);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(0);
  expect(await umbracoApi.document.getChildrenAmount(secondDocumentId)).toEqual(2);
});

test('can bulk trash multiple child content items in list view', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentCardWithName(firstChildContentName);
  await umbracoUi.content.selectContentCardWithName(secondChildContentName);
  await umbracoUi.content.clickTrashSelectedListItems();
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  await umbracoUi.content.doesListViewContainCount(0);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(0);
  await umbracoUi.content.isItemVisibleInRecycleBin(firstChildContentName);
  await umbracoUi.content.isItemVisibleInRecycleBin(secondChildContentName);
});

test('can bulk publish multiple child content items when some are already published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  await umbracoApi.document.publish(documentId);
  await umbracoApi.document.publish(firstChildId);
  const firstChildBeforePublish = await umbracoApi.document.getByName(firstChildContentName);
  expect(firstChildBeforePublish.variants[0].state).toBe('Published');
  const secondChildBeforePublish = await umbracoApi.document.getByName(secondChildContentName);
  expect(secondChildBeforePublish.variants[0].state).toBe('Draft');
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentCardWithName(firstChildContentName);
  await umbracoUi.content.selectContentCardWithName(secondChildContentName);
  await umbracoUi.content.clickPublishSelectedListItems();
  await umbracoUi.content.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  const firstChildData = await umbracoApi.document.getByName(firstChildContentName);
  expect(firstChildData.variants[0].state).toBe(expectedState);
  const secondChildData = await umbracoApi.document.getByName(secondChildContentName);
  expect(secondChildData.variants[0].state).toBe(expectedState);
});

test('can clear selection after selecting multiple child content items in list view', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.selectContentCardWithName(firstChildContentName);
  await umbracoUi.content.selectContentCardWithName(secondChildContentName);
  await umbracoUi.content.isCollectionSelectionActionsVisible();
  await umbracoUi.content.clickClearSelectionButton();

  // Assert
  await umbracoUi.content.isCollectionSelectionActionsVisible(false);
  await umbracoUi.content.isContentCardWithNameSelected(firstChildContentName, false);
  await umbracoUi.content.isContentCardWithNameSelected(secondChildContentName, false);
  expect(await umbracoApi.document.getChildrenAmount(documentId)).toEqual(2);
});
