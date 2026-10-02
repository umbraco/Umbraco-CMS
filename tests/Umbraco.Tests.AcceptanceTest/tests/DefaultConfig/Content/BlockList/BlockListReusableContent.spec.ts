import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContentReusable';
const secondContentName = 'TestContentReusableSecond';
const documentTypeName = 'TestDocumentTypeForReusableContent';
const customDataTypeName = 'Custom Block List Reusable';
const elementTypeName = 'BlockListReusableElement';
const libraryElementName = 'MyLibraryElement';
const transferElementName = 'TransferredLibraryElement';
const libraryFolderName = 'TestReusableFolder';
const propertyInBlock = 'Textstring';
const groupName = 'testGroup';
const blockListEditorAlias = 'Umbraco.BlockList';
const elementPickerDataTypeName = 'Element Picker For Reusable Usage';
const pickerContentName = 'PickerReferencingContent';
const pickerDocumentTypeName = 'PickerReferencingDocumentType';
const templateName = 'ReusableBlockCrossDocTemplate';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  const textStringData = await umbracoApi.dataType.getByName(propertyInBlock);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, propertyInBlock, textStringData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(contentName + ' (1)');
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.element.ensureNameNotExists(libraryElementName);
  await umbracoApi.element.ensureNameNotExists(transferElementName);
  await umbracoApi.element.ensureNameNotExists(libraryFolderName);
  await umbracoApi.document.ensureNameNotExists(pickerContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(pickerDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(elementPickerDataTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can insert a block from the Library', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.isBlockLinkIconVisible(true);
  await umbracoUi.content.isBlockMarkedAsReference(true);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockListValue = await umbracoApi.document.getBlockListValue(contentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
});

test('can disconnect a block from the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, 'Shared library text', propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.clickDisconnectFromLibraryBlockButton();
  await umbracoUi.content.clickConfirmDisconnectFromLibraryButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockListValue = await umbracoApi.document.getBlockListValue(contentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).not.toBe(true);
  expect(layoutItem.contentKey).not.toBe(libraryElementId);
  // The Library element still exists
  expect(await umbracoApi.element.doesNameExist(libraryElementName)).toBeTruthy();
});

test('can edit the shared Library elements content from a referenced block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const editedText = 'Edited in place';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, 'Initial library text', propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.clickEditBlockListBlockButton();
  await umbracoUi.content.enterTextstring(editedText);
  await umbracoUi.content.clickSaveInReferencedElementWorkspace();

  // Assert
  await umbracoApi.element.waitUntilFirstPropertyValueEquals(libraryElementId, editedText);
});

test('shows a draft indicator on a block referencing an unpublished Library element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  await umbracoUi.content.doesBlockHaveDraftTag(true);
});

test('references the same Library element in multiple blocks with a shared content key', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockListValue = await umbracoApi.document.getBlockListValue(contentName);
  const layout = blockListValue.layout[blockListEditorAlias];
  expect(layout.length).toBe(2);
  expect(layout[0].isExternalContent).toBe(true);
  expect(layout[1].isExternalContent).toBe(true);
  expect(layout[0].contentKey).toBe(libraryElementId);
  expect(layout[1].contentKey).toBe(libraryElementId);
  expect(layout[0].key).not.toBe(layout[1].key);
});

test('preserves the block content values when disconnecting from the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const sharedText = 'Shared library text';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, sharedText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.clickDisconnectFromLibraryBlockButton();
  await umbracoUi.content.clickConfirmDisconnectFromLibraryButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockListValue = await umbracoApi.document.getBlockListValue(contentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).not.toBe(true);
  expect(umbracoApi.document.getBlockContentPropertyValue(blockListValue, layoutItem.contentKey)).toBe(sharedText);
});

test('updates the draft indicator when the referenced Library element is published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.doesBlockHaveDraftTag(true);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.doesBlockHaveDraftTag(false);
});

test('cannot confirm a transfer until both a name and a location are provided', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createDefaultElementFolder(libraryFolderName);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring('Local block content');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickTransferToLibraryBlockButton();

  // Assert
  await umbracoUi.content.isConfirmTransferToLibraryButtonEnabled(false);
  await umbracoUi.content.enterNameInTransferToLibraryModal(transferElementName);
  await umbracoUi.content.isConfirmTransferToLibraryButtonEnabled(false);
  await umbracoUi.content.selectFolderInTransferToLibraryModal(libraryFolderName);
  await umbracoUi.content.isConfirmTransferToLibraryButtonEnabled(true);
});

test('can transfer a local block to the Library', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const localBlockText = 'Local block content';
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(localBlockText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickTransferToLibraryBlockButton();
  await umbracoUi.content.transferBlockToLibraryRoot(transferElementName);
  await umbracoUi.content.isBlockMarkedAsReference(true);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const transferredElement = await umbracoApi.element.getByName(transferElementName);
  expect(transferredElement).toBeTruthy();
  const blockListValue = await umbracoApi.document.getBlockListValue(contentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(transferredElement.id);
});

test('removes the block from the editor when the referenced Library element is deleted', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.isBlockEntryVisible(true);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Act
  await umbracoApi.element.delete(libraryElementId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.isBlockEntryVisible(false);
});

test('keeps the Library reference when the content is duplicated', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const duplicatedContentName = contentName + ' (1)';
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Act - duplicate the document to root
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickDuplicateToActionMenuOption();
  await umbracoUi.content.clickLabelWithName('Content');
  await umbracoUi.content.clickCopyModalButton();
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);

  // Assert
  expect(await umbracoApi.document.doesNameExist(duplicatedContentName)).toBeTruthy();
  const blockListValue = await umbracoApi.document.getBlockListValue(duplicatedContentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
});

test('shows the referencing content in the Element info tab when referenced via an Element Picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  const pickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const pickerDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(pickerDocumentTypeName, elementPickerDataTypeName, pickerDataTypeId);
  await umbracoApi.document.createDocumentWithElementPickers(pickerContentName, pickerDocumentTypeId, elementPickerDataTypeName, [libraryElementId]);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(libraryElementName);
  await umbracoUi.library.clickInfoTab();

  // Assert
  await umbracoUi.library.doesReferencesItemsInInfoTabHaveCount(1);
});

test('shows the referencing content in the Element info tab when referenced via a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(libraryElementName);
  await umbracoUi.library.clickInfoTab();

  // Assert
  await umbracoUi.library.doesReferencesItemsInInfoTabHaveCount(1);
});

test('can transfer a local block to a Library folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const localBlockText = 'Local block content for folder transfer';
  const folderId = await umbracoApi.element.createDefaultElementFolder(libraryFolderName);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(localBlockText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickTransferToLibraryBlockButton();
  await umbracoUi.content.transferBlockToLibraryFolder(transferElementName, libraryFolderName);
  await umbracoUi.content.isBlockMarkedAsReference(true);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const transferredElement = await umbracoApi.element.getByName(transferElementName);
  expect(transferredElement).toBeTruthy();
  const folderChildren = await umbracoApi.element.getChildren(folderId);
  expect(folderChildren.some(child => child.id === transferredElement.id)).toBeTruthy();
  const blockListValue = await umbracoApi.document.getBlockListValue(contentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(transferredElement.id);
});

test('disconnecting a block in one document leaves the reference intact in another document', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  const blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(customDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, blockListDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocument(secondContentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(secondContentName);
  await umbracoUi.reloadPage();
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickDisconnectFromLibraryBlockButton();
  await umbracoUi.content.clickConfirmDisconnectFromLibraryButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const firstValue = await umbracoApi.document.getBlockListValue(contentName);
  const firstLayoutItem = firstValue.layout[blockListEditorAlias][0];
  expect(firstLayoutItem.isExternalContent).not.toBe(true);
  expect(firstLayoutItem.contentKey).not.toBe(libraryElementId);
  const secondValue = await umbracoApi.document.getBlockListValue(secondContentName);
  const secondLayoutItem = secondValue.layout[blockListEditorAlias][0];
  expect(secondLayoutItem.isExternalContent).toBe(true);
  expect(secondLayoutItem.contentKey).toBe(libraryElementId);
  expect(await umbracoApi.element.doesNameExist(libraryElementName)).toBeTruthy();
});

test('shares a Library element across two documents and reflects updates in both', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const initialText = 'Shared reusable text';
  const updatedText = 'Shared reusable text (updated)';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, initialText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingBlockListItems(templateName, customDataTypeName, propertyInBlock);
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(customDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, customDataTypeId, customDataTypeName, templateId);
  const firstDocumentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const secondDocumentId = await umbracoApi.document.createDefaultDocument(secondContentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(secondContentName);
  await umbracoUi.reloadPage();
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  const firstValue = await umbracoApi.document.getBlockListValue(contentName);
  const secondValue = await umbracoApi.document.getBlockListValue(secondContentName);
  expect(firstValue.layout[blockListEditorAlias][0].contentKey).toBe(libraryElementId);
  expect(secondValue.layout[blockListEditorAlias][0].contentKey).toBe(libraryElementId);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(libraryElementName);
  await umbracoUi.library.clickInfoTab();
  await umbracoUi.library.doesReferencesItemsInInfoTabHaveCount(2);

  // Act
  await umbracoApi.element.updateFirstPropertyValueAndPublish(libraryElementId, updatedText);

  // Assert
  const firstUrl = await umbracoApi.document.getDocumentUrl(firstDocumentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(firstUrl);
  await umbracoUi.contentRender.doesContentRenderValueContainText(updatedText);
  const secondUrl = await umbracoApi.document.getDocumentUrl(secondDocumentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(secondUrl);
  await umbracoUi.contentRender.doesContentRenderValueContainText(updatedText);
});
