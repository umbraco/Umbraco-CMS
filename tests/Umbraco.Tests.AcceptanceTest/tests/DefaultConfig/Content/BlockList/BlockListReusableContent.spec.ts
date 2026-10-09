import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContentReusable';
const secondContentName = 'TestContentReusableSecond';
const documentTypeName = 'TestDocumentTypeForReusableContent';
const customDataTypeName = 'Custom Block List Reusable';
const elementTypeName = 'BlockListReusableElement';
const settingsElementTypeName = 'BlockListReusableSettings';
const libraryElementName = 'MyLibraryElement';
const transferElementName = 'TransferredLibraryElement';
const libraryFolderName = 'TestReusableFolder';
const propertyInBlock = 'Textstring';
const settingsPropertyName = 'Textarea';
const groupName = 'testGroup';
const blockListEditorAlias = 'Umbraco.BlockList';
const templateName = 'ReusableBlockCrossDocTemplate';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  const textStringData = await umbracoApi.dataType.getByName(propertyInBlock);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, propertyInBlock, textStringData.id);
  await umbracoApi.element.ensureNameNotExists(transferElementName);
  await umbracoApi.document.ensureNameNotExists(contentName + ' (1)');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(contentName + ' (1)');
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.element.ensureNameNotExists(libraryElementName);
  await umbracoApi.element.ensureNameNotExists(transferElementName);
  await umbracoApi.element.ensureNameNotExists(libraryFolderName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(settingsElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
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
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockListValue = await umbracoApi.document.getBlockListValue(contentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
});

test('can disconnect a block from the Library', async ({umbracoApi, umbracoUi}) => {
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
  expect(layoutItem.contentKey).not.toBe(libraryElementId);
  expect(umbracoApi.document.getBlockContentPropertyValue(blockListValue, layoutItem.contentKey)).toBe(sharedText);
  // The Library element still exists
  expect(await umbracoApi.element.doesNameExist(libraryElementName)).toBeTruthy();
});

test('can edit the shared Library element content from a referenced block', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickEditBlockButton();
  await umbracoUi.content.enterTextstringInReferencedElementWorkspace(editedText);
  await umbracoUi.content.clickSaveInReferencedElementWorkspace();

  // Assert
  await umbracoApi.element.waitUntilFirstPropertyValueEquals(libraryElementId, editedText);
});

test('can edit the settings of a referenced block without changing the Library element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const sharedText = 'Shared library text';
  const settingsText = 'Local settings value';
  const textAreaData = await umbracoApi.dataType.getByName(settingsPropertyName);
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingsElementTypeName, groupName, settingsPropertyName, textAreaData.id);
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, sharedText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditorWithSettings(contentName, elementTypeId, settingsElementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickEditSettingsBlockButton();
  await umbracoUi.content.enterTextArea(settingsText);
  await umbracoUi.content.clickUpdateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockListValue = await umbracoApi.document.getBlockListValue(contentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
  expect(umbracoApi.document.getBlockSettingsPropertyValue(blockListValue, layoutItem.settingsKey)).toBe(settingsText);
  expect(await umbracoApi.element.getFirstPropertyValue(libraryElementId)).toBe(sharedText);
});

test('can disconnect a block from the Library and keep its local settings', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const settingsText = 'Local settings value';
  const textAreaData = await umbracoApi.dataType.getByName(settingsPropertyName);
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingsElementTypeName, groupName, settingsPropertyName, textAreaData.id);
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditorWithSettings(contentName, elementTypeId, settingsElementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickEditSettingsBlockButton();
  await umbracoUi.content.enterTextArea(settingsText);
  await umbracoUi.content.clickUpdateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Act
  await umbracoUi.content.clickDisconnectFromLibraryBlockButton();
  await umbracoUi.content.clickConfirmDisconnectFromLibraryButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockListValue = await umbracoApi.document.getBlockListValue(contentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).not.toBe(true);
  expect(umbracoApi.document.getBlockSettingsPropertyValue(blockListValue, layoutItem.settingsKey)).toBe(settingsText);
});

test('can duplicate content and keep the local settings of a referenced block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const settingsText = 'Local settings value';
  const duplicatedContentName = contentName + ' (1)';
  const textAreaData = await umbracoApi.dataType.getByName(settingsPropertyName);
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingsElementTypeName, groupName, settingsPropertyName, textAreaData.id);
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditorWithSettings(contentName, elementTypeId, settingsElementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickEditSettingsBlockButton();
  await umbracoUi.content.enterTextArea(settingsText);
  await umbracoUi.content.clickUpdateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickDuplicateToActionMenuOption();
  await umbracoUi.content.clickLabelWithName('Content');
  await umbracoUi.content.clickCopyModalButton();
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);

  // Assert
  const blockListValue = await umbracoApi.document.getBlockListValue(duplicatedContentName);
  const layoutItem = blockListValue.layout[blockListEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
  expect(umbracoApi.document.getBlockSettingsPropertyValue(blockListValue, layoutItem.settingsKey)).toBe(settingsText);
});

test('can see a draft indicator on a block referencing an unpublished Library element', async ({umbracoApi, umbracoUi}) => {
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

test('can reference the same Library element in multiple blocks with a shared content key', async ({umbracoApi, umbracoUi}) => {
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

test('can see the draft indicator disappear when the referenced Library element is published', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.enterNameInTransferToLibraryModal('');
  await umbracoUi.content.isConfirmTransferToLibraryButtonEnabled(false);
  await umbracoUi.content.enterNameInTransferToLibraryModal(transferElementName);
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

test('cannot see a block whose referenced Library element was deleted', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoApi.element.deleteAndVerifyElementIsDeleted(libraryElementId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithNameAndWaitForReferencedElementResponse(contentName, libraryElementId, ConstantHelper.statusCodes.notFound);

  // Assert
  await umbracoUi.content.isAddBlockElementButtonVisible();
  await umbracoUi.content.isBlockEntryVisible(false);
});

test('can duplicate content and keep the Library reference', async ({umbracoApi, umbracoUi}) => {
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

test('can see the referencing content in the Element info tab when referenced via a block', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.library.isReferenceItemNameVisible(contentName);
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

test('can disconnect a block in one document and leave the reference intact in another document', async ({umbracoApi, umbracoUi}) => {
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

test('can share a Library element across two documents and reflect updates in both', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.library.isReferenceItemNameVisible(contentName);
  await umbracoUi.library.isReferenceItemNameVisible(secondContentName);

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
