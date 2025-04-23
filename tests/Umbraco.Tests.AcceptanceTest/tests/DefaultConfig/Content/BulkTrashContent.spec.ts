import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let collectionId = '';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const childDocumentTypeName = 'TestChildDocumentType';
const firstChildContentName = 'First Child Content';
const secondChildContentName = 'Second Child Content';
const collectionDataTypeName = 'List View - Content';
const referenceHeadline = ConstantHelper.trashDeleteDialogMessage.bulkReferenceHeadline;
const documentPickerName = ['TestPicker', 'DocumentTypeForPicker'];

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  const collectionDataTypeData = await umbracoApi.dataType.getByName(collectionDataTypeName);
  collectionId = collectionDataTypeData.id;
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.document.emptyRecycleBin();
});

test('can bulk trash content nodes without a relation', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, collectionId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, contentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.selectContentWithNameInListView(firstChildContentName);
  await umbracoUi.content.selectContentWithNameInListView(secondChildContentName);
  await umbracoUi.content.clickTrashSelectedListItems();
  // Verify the references list not displayed
  await umbracoUi.content.isReferenceHeadlineVisible(false);
  await umbracoUi.content.clickConfirmTrashButton();

  // // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(firstChildContentName)).toBeFalsy();
  expect(await umbracoApi.document.doesNameExist(secondChildContentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(firstChildContentName);
  await umbracoUi.content.isItemVisibleInRecycleBin(secondChildContentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(firstChildContentName)).toBeTruthy();
  expect(await umbracoApi.document.doesItemExistInRecycleBin(secondChildContentName)).toBeTruthy();
});

test('can bulk trash content nodes with a relation', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, collectionId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  const firstChildContentId = await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, contentId);
  await umbracoApi.document.publish(firstChildContentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildContentName, childDocumentTypeId, contentId);
  // Create a document that have document picker is firstChildContentName
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], firstChildContentName, firstChildContentId, documentPickerName[1]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.selectContentWithNameInListView(firstChildContentName);
  await umbracoUi.content.selectContentWithNameInListView(secondChildContentName);
  await umbracoUi.content.clickTrashSelectedListItems();
  // Verify the references list
  await umbracoUi.content.doesReferenceHeadlineHaveText(referenceHeadline);
  await umbracoUi.content.doesReferenceItemsHaveCount(1);
  await umbracoUi.content.isReferenceItemNameVisible(firstChildContentName);
  await umbracoUi.content.clickConfirmTrashButton();

  // // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(firstChildContentName)).toBeFalsy();
  expect(await umbracoApi.document.doesNameExist(secondChildContentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(firstChildContentName);
  await umbracoUi.content.isItemVisibleInRecycleBin(secondChildContentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(firstChildContentName)).toBeTruthy();
  expect(await umbracoApi.document.doesItemExistInRecycleBin(secondChildContentName)).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName[1]);
});