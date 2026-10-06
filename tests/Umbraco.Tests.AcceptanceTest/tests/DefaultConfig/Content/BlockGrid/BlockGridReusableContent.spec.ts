import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContentReusableGrid';
const documentTypeName = 'TestDocumentTypeForReusableGrid';
const customDataTypeName = 'Custom Block Grid Reusable';
const elementTypeName = 'BlockGridReusableElement';
const settingsElementTypeName = 'BlockGridReusableSettings';
const libraryElementName = 'MyGridLibraryElement';
const transferElementName = 'TransferredGridLibraryElement';
const propertyInBlock = 'Textstring';
const settingsPropertyName = 'Textarea';
const groupName = 'testGroup';
const blockGridEditorAlias = 'Umbraco.BlockGrid';
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
  await umbracoApi.element.ensureNameNotExists(libraryElementName);
  await umbracoApi.element.ensureNameNotExists(transferElementName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(settingsElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can insert a block from the Library', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.isBlockLinkIconVisible(true, 'grid');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockGridValue = await umbracoApi.document.getBlockGridValue(contentName);
  const layoutItem = blockGridValue.layout[blockGridEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
});

test('can disconnect a block from the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const sharedText = 'Shared grid library text';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, sharedText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.clickDisconnectFromLibraryBlockButton('grid');
  await umbracoUi.content.clickConfirmDisconnectFromLibraryButton('grid');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockGridValue = await umbracoApi.document.getBlockGridValue(contentName);
  const layoutItem = blockGridValue.layout[blockGridEditorAlias][0];
  expect(layoutItem.isExternalContent).not.toBe(true);
  expect(layoutItem.contentKey).not.toBe(libraryElementId);
  expect(umbracoApi.document.getBlockContentPropertyValue(blockGridValue, layoutItem.contentKey)).toBe(sharedText);
  expect(await umbracoApi.element.doesNameExist(libraryElementName)).toBeTruthy();
});

test('can edit the shared Library element content from a referenced block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const editedText = 'Edited grid in place';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, 'Initial grid library text', propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.clickEditBlockButton('grid');
  await umbracoUi.content.enterTextstringInReferencedElementWorkspace(editedText);
  await umbracoUi.content.clickSaveInReferencedElementWorkspace();

  // Assert
  await umbracoApi.element.waitUntilFirstPropertyValueEquals(libraryElementId, editedText);
});

test('can edit the settings of a referenced block without changing the Library element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const sharedText = 'Shared grid library text';
  const settingsText = 'Local grid settings value';
  const textAreaData = await umbracoApi.dataType.getByName(settingsPropertyName);
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingsElementTypeName, groupName, settingsPropertyName, textAreaData.id);
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, sharedText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockGridEditorWithSettings(contentName, elementTypeId, settingsElementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.clickEditSettingsBlockButton('grid');
  await umbracoUi.content.enterTextArea(settingsText);
  await umbracoUi.content.clickUpdateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockGridValue = await umbracoApi.document.getBlockGridValue(contentName);
  const layoutItem = blockGridValue.layout[blockGridEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
  expect(umbracoApi.document.getBlockSettingsPropertyValue(blockGridValue, layoutItem.settingsKey)).toBe(settingsText);
  expect(await umbracoApi.element.getFirstPropertyValue(libraryElementId)).toBe(sharedText);
});

test('can see a draft indicator on a block referencing an unpublished Library element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  await umbracoUi.content.doesBlockHaveDraftTag(true, 'grid');
});

test('can see the draft indicator disappear when the referenced Library element is published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.doesBlockHaveDraftTag(true, 'grid');

  // Publish the Library element and reload the content
  await umbracoApi.element.publish(libraryElementId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.doesBlockHaveDraftTag(false, 'grid');
});

test('can transfer a local block to the Library', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const localBlockText = 'Local grid block content';
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(localBlockText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickTransferToLibraryBlockButton('grid');
  await umbracoUi.content.transferBlockToLibraryRoot(transferElementName);
  await umbracoUi.content.isBlockMarkedAsReference(true, 'grid');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const transferredElement = await umbracoApi.element.getByName(transferElementName);
  expect(transferredElement).toBeTruthy();
  const blockGridValue = await umbracoApi.document.getBlockGridValue(contentName);
  const layoutItem = blockGridValue.layout[blockGridEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(transferredElement.id);
});

test('can duplicate content and keep the Library reference', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const duplicatedContentName = contentName + ' (1)';
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickDuplicateToActionMenuOption();
  await umbracoUi.content.clickLabelWithName('Content');
  await umbracoUi.content.clickCopyModalButton();
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);

  // Assert
  expect(await umbracoApi.document.doesNameExist(duplicatedContentName)).toBeTruthy();
  const blockGridValue = await umbracoApi.document.getBlockGridValue(duplicatedContentName);
  const layoutItem = blockGridValue.layout[blockGridEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
});

test('can reference the same Library element in multiple blocks with a shared content key', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blockGridValue = await umbracoApi.document.getBlockGridValue(contentName);
  const layout = blockGridValue.layout[blockGridEditorAlias];
  expect(layout.length).toBe(2);
  expect(layout[0].isExternalContent).toBe(true);
  expect(layout[1].isExternalContent).toBe(true);
  expect(layout[0].contentKey).toBe(libraryElementId);
  expect(layout[1].contentKey).toBe(libraryElementId);
  expect(layout[0].key).not.toBe(layout[1].key);
});
