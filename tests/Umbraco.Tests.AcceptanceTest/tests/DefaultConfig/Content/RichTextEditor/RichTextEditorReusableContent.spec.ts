import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContentReusableRte';
const documentTypeName = 'TestDocumentTypeForReusableRte';
const customDataTypeName = 'Custom RichText Reusable';
const elementTypeName = 'RteReusableElement';
const secondElementTypeName = 'RteReusableSecondElement';
const settingsElementTypeName = 'RteReusableSettings';
const libraryElementName = 'MyRteLibraryElement';
const transferElementName = 'TransferredRteLibraryElement';
const propertyInBlock = 'Textstring';
const settingsPropertyName = 'Textarea';
const groupName = 'testGroup';
const richTextBlockEditorAlias = 'Umbraco.RichText';
let elementTypeId = '';
let blockElementTypeIds: string[] = [];

test.beforeEach(async ({umbracoApi}) => {
  const textStringData = await umbracoApi.dataType.getByName(propertyInBlock);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, propertyInBlock, textStringData.id);
  // With a single block type the toolbar's Insert Block skips the catalogue and opens the create workspace,
  // so a second block type keeps the catalogue - and its Library tab - as the first thing that opens.
  const secondElementTypeId = await umbracoApi.documentType.createEmptyElementType(secondElementTypeName);
  blockElementTypeIds = [elementTypeId, secondElementTypeId];
  await umbracoApi.element.ensureNameNotExists(transferElementName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.element.ensureNameNotExists(libraryElementName);
  await umbracoApi.element.ensureNameNotExists(transferElementName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(settingsElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can insert a block from the Library', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, blockElementTypeIds, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'rte');
  await umbracoUi.content.isBlockLinkIconVisible(true, 'rte');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blocksValue = await umbracoApi.document.getRichTextBlocksValue(contentName);
  const layoutItem = blocksValue.layout[richTextBlockEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
});

test('can transfer a local block to the Library', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const localBlockText = 'Local rte block content';
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, blockElementTypeIds, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButtonInRte();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(localBlockText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickTransferToLibraryBlockButton('rte');
  await umbracoUi.content.transferBlockToLibraryRoot(transferElementName);
  await umbracoUi.content.isBlockMarkedAsReference(true, 'rte');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const transferredElement = await umbracoApi.element.getByName(transferElementName);
  expect(transferredElement).toBeTruthy();
  const blocksValue = await umbracoApi.document.getRichTextBlocksValue(contentName);
  const layoutItem = blocksValue.layout[richTextBlockEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(transferredElement.id);
});

test('can disconnect a block from the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const sharedText = 'Shared rte library text';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, sharedText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, blockElementTypeIds, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'rte');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.clickDisconnectFromLibraryBlockButton('rte');
  await umbracoUi.content.clickConfirmDisconnectFromLibraryButton('rte');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blocksValue = await umbracoApi.document.getRichTextBlocksValue(contentName);
  const layoutItem = blocksValue.layout[richTextBlockEditorAlias][0];
  expect(layoutItem.isExternalContent).not.toBe(true);
  expect(layoutItem.contentKey).not.toBe(libraryElementId);
  expect(umbracoApi.document.getBlockContentPropertyValue(blocksValue, layoutItem.contentKey)).toBe(sharedText);
  expect(await umbracoApi.element.doesNameExist(libraryElementName)).toBeTruthy();
});

test('can edit the shared Library element content from a referenced block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const editedText = 'Edited rte in place';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, 'Initial rte library text', propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, blockElementTypeIds, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'rte');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.clickEditBlockButton('rte');
  await umbracoUi.content.enterTextstringInReferencedElementWorkspace(editedText);
  await umbracoUi.content.clickSaveInReferencedElementWorkspace();

  // Assert
  await umbracoApi.element.waitUntilFirstPropertyValueEquals(libraryElementId, editedText);
});

test('can edit the settings of a referenced block without changing the Library element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const sharedText = 'Shared rte library text';
  const settingsText = 'Local rte settings value';
  const textAreaData = await umbracoApi.dataType.getByName(settingsPropertyName);
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingsElementTypeName, groupName, settingsPropertyName, textAreaData.id);
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, sharedText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditorWithSettings(contentName, blockElementTypeIds, settingsElementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'rte');
  await umbracoUi.content.clickEditSettingsBlockButton('rte');
  await umbracoUi.content.enterTextArea(settingsText);
  await umbracoUi.content.clickUpdateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const blocksValue = await umbracoApi.document.getRichTextBlocksValue(contentName);
  const layoutItem = blocksValue.layout[richTextBlockEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
  expect(umbracoApi.document.getBlockSettingsPropertyValue(blocksValue, layoutItem.settingsKey)).toBe(settingsText);
  expect(await umbracoApi.element.getFirstPropertyValue(libraryElementId)).toBe(sharedText);
});

test('can see a draft indicator on a block referencing an unpublished Library element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, blockElementTypeIds, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'rte');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  await umbracoUi.content.doesBlockHaveDraftTag(true, 'rte');
});

test('can see the draft indicator disappear when the referenced Library element is published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, blockElementTypeIds, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'rte');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.doesBlockHaveDraftTag(true, 'rte');
  await umbracoApi.element.publish(libraryElementId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.doesBlockHaveDraftTag(false, 'rte');
});
