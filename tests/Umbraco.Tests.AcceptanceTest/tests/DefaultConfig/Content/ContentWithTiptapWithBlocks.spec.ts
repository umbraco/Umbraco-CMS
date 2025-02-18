import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
let documentTypeId = null;
const tipTapDataTypeName = 'Test RTE Tiptap';
const tipTapDataTypeAlias = AliasHelper.toAlias(tipTapDataTypeName);
let tipTapDataTypeId = null;

// Element Type
const elementTypeName = 'TestElement';
const elementGroupName = 'TestElementGroup';
let elementTypeId = null;
const textStringDataTypeName = 'Textstring';
const textStringDataTypeAlias = AliasHelper.toAlias(textStringDataTypeName);
let textStringDataTypeId = null;
const propertyValue = 'Test Tiptap here';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(tipTapDataTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringDataTypeId = textStringDataType.id;
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, elementGroupName, textStringDataTypeName, textStringDataTypeId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(tipTapDataTypeName);
});


test('can create content with Tiptap that has a block with default settings', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  tipTapDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlock(tipTapDataTypeName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapDataTypeName, tipTapDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterPropertyValue(textStringDataTypeName, propertyValue);
  await umbracoUi.content.clickCreateButtonForModalWithElementTypeNameAndGroupName(elementTypeName, elementTypeName);

  // Assert
  await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.document.doesTipTapDataTypeWithNameContainBlockWithValue(contentName, tipTapDataTypeAlias, elementTypeId, textStringDataTypeAlias, propertyValue)).toBeTruthy();
});

test('can create content with Tiptap that has a block with a Label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockLabel = 'Test Label';
  tipTapDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlockWithBlockSettingLabel(tipTapDataTypeName, elementTypeId, blockLabel);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapDataTypeName, tipTapDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterPropertyValue(textStringDataTypeName, propertyValue);
  await umbracoUi.content.clickCreateButtonForModalWithElementTypeNameAndGroupName(elementTypeName, elementTypeName);

  // Assert
  await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesRichTextEditorBlockContainLabel(tipTapDataTypeAlias, blockLabel);
});

test('can create content with Tiptap that has a block with display line enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  tipTapDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlockWithBlockSettingDisplayInline(tipTapDataTypeName, elementTypeId, true);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapDataTypeName, tipTapDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterPropertyValue(textStringDataTypeName, propertyValue);
  await umbracoUi.content.clickCreateButtonForModalWithElementTypeNameAndGroupName(elementTypeName, elementTypeName);

  // Assert
  await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesBlockEditorModalContainInline(tipTapDataTypeAlias, elementTypeName);
});

test('can create content with Tiptap that has a block with an updated editor size', async ({umbracoApi, umbracoUi}) => {
// Arrange
  const editorSize = 'full';
  tipTapDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlockWithBlockSettingEditorSize(tipTapDataTypeName, elementTypeId, editorSize);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapDataTypeName, tipTapDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);

  // Assert
  await umbracoUi.content.doesBlockEditorModalContainEditorSize(editorSize, elementTypeName);
});
test('can create content with Tiptap that has a block with a background color', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backgroundColor = '#ff0000';
  tipTapDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlockWithBlockSettingBackgroundColor(tipTapDataTypeName, elementTypeId, backgroundColor);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapDataTypeName, tipTapDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButton();

  // Assert
  await umbracoUi.content.doesBlockHaveBackgroundColor(elementTypeName, backgroundColor);
});

test('can create content with Tiptap that has a block with a icon color', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backgroundColor = '#ff8000';
  tipTapDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlockWithBlockSettingIconColor(tipTapDataTypeName, elementTypeId, backgroundColor);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapDataTypeName, tipTapDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButton();

  // Assert
  await umbracoUi.content.doesBlockHaveIconColor(elementTypeName, backgroundColor);
});

test.skip('can create content with Tiptap that has a block with a thumbnail', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const thumbnail = 'icon-document';
  tipTapDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlockWithBlockSettingThumbnail(tipTapDataTypeName, elementTypeId, thumbnail);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapDataTypeName, tipTapDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButton();

  // Assert
  // await umbracoUi.content.doesBlockHaveThumbnail(elementTypeName, thumbnail);
});
