import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestContent';
const contentText = 'This is test content text';
const firstChildContentName = 'First Child Content';
const secondChildContentName = 'Second Child Content';
// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
const childDocumentTypeName = 'TestChildDocumentType';
// Media
const mediaFileName = 'TestMediaFile';
// Data Type
let dataTypeId = '';
const dataTypeName = 'Textstring';
const collectionDataTypeName = 'List View - Content';
// Pickers
const documentPickerName = ['TestPicker', 'DocumentTypeForPicker'];
const mediaPickerDocumentName = ['MediaTestPicker', 'DocumentTypeForMediaPicker'];
// Warning message
const warningMessage = (itemName) => `${itemName} cannot be moved to the Recycle Bin because it is referenced by other items.`;
const warningMessageForBulk = (itemCount) => `The selected ${itemCount} cannot be moved to the Recycle Bin because at least one item is referenced by other content.`;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataTypeData.id;
  await umbracoApi.document.emptyRecycleBin();
  await umbracoApi.media.emptyRecycleBin();
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.document.ensureNameNotExists(secondChildContentName);
  await umbracoApi.document.ensureNameNotExists(documentPickerName[0]);
  await umbracoApi.document.ensureNameNotExists(mediaPickerDocumentName[0]);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName[1]);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(mediaPickerDocumentName[1]);
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  await umbracoApi.document.emptyRecycleBin();
  await umbracoApi.media.emptyRecycleBin();
});

test('can empty recycle bin when trashed item has no references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.moveToRecycleBin(contentId);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickEmptyRecycleBinButton();
  await umbracoUi.content.clickConfirmEmptyRecycleBinButtonAndWaitForRecycleBinToBeEmptied();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeFalsy();
});

test('can trash an invariant content node without references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashActionMenuOption();
  await umbracoUi.content.clickConfirmTrashButtonAndWaitForContentToBeTrashed();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('can trash an variant content node without references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashActionMenuOption();
  await umbracoUi.content.clickConfirmTrashButtonAndWaitForContentToBeTrashed();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('cannot trash an invariant content node that has references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create an invariant published content node
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  // Create a document that references the content via Multi URL Picker
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], contentName, contentId, documentPickerName[1]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashActionMenuOption();

  // Assert
  await umbracoUi.content.isConfirmTrashButtonDisabled();
  await umbracoUi.content.doesReferenceHeadlineHaveText(ConstantHelper.trashDeleteDialogMessage.referenceHeadline);
  await umbracoUi.content.doesModalHaveText(warningMessage(contentName));
  await umbracoUi.content.doesReferenceItemsHaveCount(1);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName[0]);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});

test('cannot trash a variant content node that has references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create a variant published content node
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publishDocumentWithCulture(contentId, 'en-US');
  // Create a document that references the content via Multi URL Picker
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], contentName, contentId, documentPickerName[1]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashActionMenuOption();

  // Assert
  await umbracoUi.content.isConfirmTrashButtonDisabled();
  await umbracoUi.content.doesReferenceHeadlineHaveText(ConstantHelper.trashDeleteDialogMessage.referenceHeadline);
  await umbracoUi.content.doesModalHaveText(warningMessage(contentName));
  await umbracoUi.content.doesReferenceItemsHaveCount(1);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName[0]);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});

test('cannot bulk trash content nodes when at least one has references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionDataTypeData = await umbracoApi.dataType.getByName(collectionDataTypeName);
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, collectionDataTypeData.id);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  const firstChildContentId = await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, contentId);
  await umbracoApi.document.publish(firstChildContentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildContentName, childDocumentTypeId, contentId);
  // Create a document that references the first child content
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], firstChildContentName, firstChildContentId, documentPickerName[1]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.selectContentCardWithName(firstChildContentName);
  await umbracoUi.content.selectContentCardWithName(secondChildContentName);
  await umbracoUi.content.clickTrashSelectedListItems();

  // Assert
  await umbracoUi.content.isConfirmTrashButtonDisabled();
  await umbracoUi.content.doesReferenceHeadlineHaveText(ConstantHelper.trashDeleteDialogMessage.bulkReferenceHeadline);
  await umbracoUi.content.doesModalHaveText(warningMessageForBulk('2 items'));
  await umbracoUi.content.doesReferenceItemsHaveCount(1);
  await umbracoUi.content.isReferenceItemNameVisible(firstChildContentName);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  expect(await umbracoApi.document.doesNameExist(firstChildContentName)).toBeTruthy();
  expect(await umbracoApi.document.doesNameExist(secondChildContentName)).toBeTruthy();
});

test('can bulk trash content nodes when none have references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionDataTypeData = await umbracoApi.dataType.getByName(collectionDataTypeName);
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, collectionDataTypeData.id);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, contentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildContentName, childDocumentTypeId, contentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.selectContentCardWithName(firstChildContentName);
  await umbracoUi.content.selectContentCardWithName(secondChildContentName);
  await umbracoUi.content.clickTrashSelectedListItems();
  await umbracoUi.content.clickConfirmTrashButtonAndWaitForContentToBeTrashed();

  // Assert
  expect(await umbracoApi.document.doesNameExist(firstChildContentName)).toBeFalsy();
  expect(await umbracoApi.document.doesNameExist(secondChildContentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(firstChildContentName);
  await umbracoUi.content.isItemVisibleInRecycleBin(secondChildContentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(firstChildContentName)).toBeTruthy();
  expect(await umbracoApi.document.doesItemExistInRecycleBin(secondChildContentName)).toBeTruthy();
});

test('cannot trash a parent content node when a child node has references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childContentName = 'ChildContent';
  // Create parent document type with allowed child node
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  // Create parent content and a child content under it
  const parentContentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const childContentId = await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, parentContentId);
  await umbracoApi.document.publish(parentContentId);
  await umbracoApi.document.publish(childContentId);
  // Create a document that references the child content via Multi URL Picker
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], childContentName, childContentId, documentPickerName[1]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashActionMenuOption();

  // Assert
  await umbracoUi.content.isConfirmTrashButtonDisabled();
  await umbracoUi.content.doesModalHaveText(warningMessage(contentName));
  await umbracoUi.content.doesReferenceHeadlineHaveText(ConstantHelper.trashDeleteDialogMessage.descendingReferenceHeadline);
  await umbracoUi.content.doesReferenceItemsHaveCount(1);
  await umbracoUi.content.isReferenceItemNameVisible(childContentName);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  expect(await umbracoApi.document.doesNameExist(childContentName)).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(childContentName);
});

test('can trash a parent content node when child node has no references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childContentName = 'ChildContent';
  // Create parent document type with allowed child node
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  // Create parent content and a child content under it
  const parentContentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, parentContentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashActionMenuOption();
  await umbracoUi.content.clickConfirmTrashButtonAndWaitForContentToBeTrashed();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(childContentName);
});

test('can trash a media item without references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.clickActionsMenuForName(mediaFileName);
  await umbracoUi.media.clickTrashActionMenuOption();
  await umbracoUi.media.clickConfirmTrashButtonAndWaitForMediaToBeTrashed();

  // Assert
  await umbracoUi.media.isMediaTreeItemVisible(mediaFileName, false);
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(mediaFileName)).toBeTruthy();
});

test('cannot trash a media item that has references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create a media file
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  // Create a document that references the media via Media Picker
  await umbracoApi.document.createDefaultDocumentWithOneMediaLink(mediaPickerDocumentName[0], mediaFileName, mediaPickerDocumentName[1]);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.clickActionsMenuForName(mediaFileName);
  await umbracoUi.media.clickTrashActionMenuOption();

  // Assert
  await umbracoUi.media.isConfirmTrashButtonDisabled();
  await umbracoUi.media.doesModalHaveText(warningMessage(mediaFileName));
  await umbracoUi.media.doesReferenceHeadlineHaveText(ConstantHelper.trashDeleteDialogMessage.referenceHeadline);
  await umbracoUi.media.doesReferenceItemsHaveCount(1);
  await umbracoUi.media.isReferenceItemNameVisible(mediaPickerDocumentName[0]);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeTruthy();
});

test('cannot bulk trash media items when at least one has references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondMediaFileName = 'SecondMediaFile';
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  await umbracoApi.media.createDefaultMediaFile(secondMediaFileName);
  // Create a document that references the first media via Media Picker
  await umbracoApi.document.createDefaultDocumentWithOneMediaLink(mediaPickerDocumentName[0], mediaFileName, mediaPickerDocumentName[1]);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.selectMediaWithName(mediaFileName);
  await umbracoUi.media.selectMediaWithName(secondMediaFileName);
  await umbracoUi.media.clickBulkTrashButton();

  // Assert
  await umbracoUi.media.isConfirmTrashButtonDisabled();
  await umbracoUi.media.doesModalHaveText(warningMessageForBulk('2 items'));
  await umbracoUi.media.doesReferenceHeadlineHaveText(ConstantHelper.trashDeleteDialogMessage.bulkReferenceHeadline);
  await umbracoUi.media.doesReferenceItemsHaveCount(1);
  await umbracoUi.media.isReferenceItemNameVisible(mediaFileName);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeTruthy();
  expect(await umbracoApi.media.doesNameExist(secondMediaFileName)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(secondMediaFileName);
});

test('can empty media recycle bin when trashed media has no references', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  await umbracoApi.media.trashMediaItem(mediaFileName);
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(mediaFileName)).toBeTruthy();
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName, true, true);
  await umbracoUi.media.clickEmptyRecycleBinButton();
  await umbracoUi.media.clickConfirmEmptyRecycleBinButtonAndWaitForRecycleBinToBeEmptied();

  // Assert
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName, false, false);
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(mediaFileName)).toBeFalsy();
});
