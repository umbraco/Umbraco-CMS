import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContentReusableRte';
const documentTypeName = 'TestDocumentTypeForReusableRte';
const customDataTypeName = 'Custom RichText Reusable';
const elementTypeName = 'RteReusableElement';
const libraryElementName = 'MyRteLibraryElement';
const transferElementName = 'TransferredRteLibraryElement';
const propertyInBlock = 'Textstring';
const groupName = 'testGroup';
const richTextBlockEditorAlias = 'Umbraco.RichText';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  const textStringData = await umbracoApi.dataType.getByName(propertyInBlock);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, propertyInBlock, textStringData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.element.ensureNameNotExists(libraryElementName);
  await umbracoApi.element.ensureNameNotExists(transferElementName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

// The four test.fixme cases below are blocked by a product bug: inserting a Library element into the
// RTE does not add the block to the editor (https://github.com/umbraco/Umbraco-CMS/issues/23381).
// They depend on a library-inserted external block and will pass once the bug is fixed. RTE Transfer
// reaches external content a different way and passes.
test.fixme('can insert a block from the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
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

test.fixme('can transfer a local block to the Library', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const localBlockText = 'Local rte block content';
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(localBlockText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickTransferToLibraryBlockButton('rte');
  await umbracoUi.content.transferBlockToLibraryRoot(transferElementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const transferredElement = await umbracoApi.element.getByName(transferElementName);
  expect(transferredElement).toBeTruthy();
  const blocksValue = await umbracoApi.document.getRichTextBlocksValue(contentName);
  const layoutItem = blocksValue.layout[richTextBlockEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(transferredElement.id);
});

test.fixme('can disconnect a block from the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, 'Shared rte library text', propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
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
  expect(await umbracoApi.element.doesNameExist(libraryElementName)).toBeTruthy();
});

test.fixme('shows a draft indicator on a block referencing an unpublished Library element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyRichTextEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'rte');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  await umbracoUi.content.doesBlockHaveDraftTag(true, 'rte');
});
