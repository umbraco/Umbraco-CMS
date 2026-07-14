import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContentReusableSingle';
const documentTypeName = 'TestDocumentTypeForReusableSingle';
const customDataTypeName = 'Custom Single Block Reusable';
const elementTypeName = 'SingleBlockReusableElement';
const libraryElementName = 'MySingleLibraryElement';
const transferElementName = 'TransferredSingleLibraryElement';
const propertyInBlock = 'Textstring';
const groupName = 'testGroup';
const singleBlockEditorAlias = 'Umbraco.SingleBlock';
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

test('can insert a block from the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptySingleBlockEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'single');
  await umbracoUi.content.isBlockLinkIconVisible(true, 'single');
  await umbracoUi.content.isBlockMarkedAsReference(true, 'single');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const singleBlockValue = await umbracoApi.document.getSingleBlockValue(contentName);
  const layoutItem = singleBlockValue.layout[singleBlockEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(libraryElementId);
});

test('can disconnect a block from the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const sharedText = 'Shared single library text';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, sharedText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptySingleBlockEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'single');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.clickDisconnectFromLibraryBlockButton('single');
  await umbracoUi.content.clickConfirmDisconnectFromLibraryButton('single');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const singleBlockValue = await umbracoApi.document.getSingleBlockValue(contentName);
  const layoutItem = singleBlockValue.layout[singleBlockEditorAlias][0];
  expect(layoutItem.isExternalContent).not.toBe(true);
  expect(layoutItem.contentKey).not.toBe(libraryElementId);
  expect(umbracoApi.document.getBlockContentPropertyValue(singleBlockValue, layoutItem.contentKey)).toBe(sharedText);
  expect(await umbracoApi.element.doesNameExist(libraryElementName)).toBeTruthy();
});

test('can edit the shared Library elements content from a referenced block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const editedText = 'Edited single in place';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, 'Initial single library text', propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptySingleBlockEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'single');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.clickEditBlockButton('single');
  await umbracoUi.content.enterTextstring(editedText);
  await umbracoUi.content.clickSaveInReferencedElementWorkspace();

  // Assert
  await umbracoApi.element.waitUntilFirstPropertyValueEquals(libraryElementId, editedText);
});

test('shows a draft indicator on a block referencing an unpublished Library element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptySingleBlockEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'single');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  await umbracoUi.content.doesBlockHaveDraftTag(true, 'single');
});

test('updates the draft indicator when the referenced Library element is published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryElementId = await umbracoApi.element.createDefaultElement(libraryElementName, elementTypeId);
  await umbracoApi.document.createDefaultDocumentWithAnEmptySingleBlockEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'single');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.doesBlockHaveDraftTag(true, 'single');

  // Publish the Library element and reload the content
  await umbracoApi.element.publish(libraryElementId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.doesBlockHaveDraftTag(false, 'single');
});

test('can transfer a local block to the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const localBlockText = 'Local single block content';
  await umbracoApi.document.createDefaultDocumentWithAnEmptySingleBlockEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(localBlockText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickTransferToLibraryBlockButton('single');
  await umbracoUi.content.transferBlockToLibraryRoot(transferElementName);
  await umbracoUi.content.isBlockMarkedAsReference(true, 'single');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const transferredElement = await umbracoApi.element.getByName(transferElementName);
  expect(transferredElement).toBeTruthy();
  const singleBlockValue = await umbracoApi.document.getSingleBlockValue(contentName);
  const layoutItem = singleBlockValue.layout[singleBlockEditorAlias][0];
  expect(layoutItem.isExternalContent).toBe(true);
  expect(layoutItem.contentKey).toBe(transferredElement.id);
});
