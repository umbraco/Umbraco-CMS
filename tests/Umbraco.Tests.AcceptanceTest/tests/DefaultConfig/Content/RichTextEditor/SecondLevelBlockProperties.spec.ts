import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content Name
const contentName = 'ContentName';

// Document Type
const documentTypeName = 'DocumentTypeName';
let documentTypeId = null;
const documentTypeGroupName = 'DocumentGroup';

// Block Grid
const richTextDataTypeName = 'RichTextDataType';
let richTextDataTypeId = null;
// Element Types
// Block List
const blockListElementTypeName = 'BlockListElementName';
let blockListElementTypeId = null;
const blockListElementGroupName = 'ListElementGroup';
// Block Grid
const blockGridElementTypeName = 'BlockGridElementName';
let blockGridElementTypeId = null;
const blockGridElementGroupName = 'GridElementGroup';
// Rich Text Editor
const richTextElementTypeName = 'RichTextElementName';
let richTextElementTypeId = null;
const richTextElementGroupName = 'RTEElementGroup';
// Text String
const textStringElementTypeName = 'TextStringElementName';
let textStringElementTypeId = null;
let textStringGroupName = 'TextGroup';
const textStringDataTypeName = 'Textstring';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
});

test('can publish a rich text editor with a rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondRichTextDataTypeName = 'SecondRichTextName';
  await umbracoApi.dataType.ensureNameNotExists(secondRichTextDataTypeName);
  const richTextEditorValue = 'Hello First World';
  const secondRichTextEditorValue = 'Hello Second World';
  const expectedRichTextEditorOutputValue = '<p>' + richTextEditorValue + '</p>';
  const secondExpectedRichTextEditorOutputValue = '<p>' + secondRichTextEditorValue + '</p>';
  const secondRichTextEditorDataTypeId = await umbracoApi.dataType.createEmptyRichTextEditor(secondRichTextDataTypeName);
  richTextElementTypeId = await umbracoApi.documentType.createDefaultElementType(richTextElementTypeName, richTextElementGroupName, secondRichTextDataTypeName, secondRichTextEditorDataTypeId);
  richTextDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlock(richTextDataTypeName, richTextElementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, richTextDataTypeName, richTextDataTypeId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.enterRTETipTapEditor(richTextEditorValue);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(richTextElementTypeName);
  await umbracoUi.content.enterRTETipTapEditorWithName(AliasHelper.toAlias(secondRichTextDataTypeName), secondRichTextEditorValue);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  // Asserts that the value in the RTE is as expected
  const documentData = await umbracoApi.document.getByName(contentName);
  const documentValues = documentData.values.find(value => value.alias === AliasHelper.toAlias(richTextDataTypeName));
  // Value in the first RTE
  expect(documentValues.value.markup).toContain(expectedRichTextEditorOutputValue);
  // Value in the second RTE
  const secondRTEInBlock = documentValues.value.blocks.contentData[0].values.find(value => value.alias === AliasHelper.toAlias(secondRichTextDataTypeName));
  expect(secondRTEInBlock.value.markup).toContain(secondExpectedRichTextEditorOutputValue);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
});

test('can publish a block list editor with a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringValue = 'Hello World';
  const blockGridDataTypeName = 'BlockGridDataTypeName';
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);

  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);
  const blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, textStringElementTypeId);

  blockGridElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockGridElementTypeName, blockGridElementGroupName, blockGridDataTypeName, blockGridDataTypeId);

  blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, blockGridElementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickLinkWithName(blockGridElementTypeName, true);
  await umbracoUi.content.clickAddBlockWithNameButton(textStringElementTypeName);
  await umbracoUi.content.clickLinkWithName(textStringElementTypeName, true);
  await umbracoUi.content.enterTextstring(textStringValue);
  await umbracoUi.content.clickCreateForModalWithHeadline('Add ' + textStringElementTypeName);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  // Asserts that the value in the BlockGrid is as expected
  const documentData = await umbracoApi.document.getByName(contentName);
  const documentValues = documentData.values.find(value => value.alias === AliasHelper.toAlias(blockListDataTypeName));
  expect(documentValues.value.contentData[0].values[0].value.contentData[0].values[0].value).toContain(textStringValue);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
});

test('can publish a block list editor with a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringValue = 'Hello World';
  const secondBlockListDataTypeName = 'SecondBlockListName';
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName)
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName)
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);

  const secondBlockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(secondBlockListDataTypeName, textStringElementTypeId);

  blockListElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockListElementTypeName, blockListElementGroupName, secondBlockListDataTypeName, secondBlockListDataTypeId);
  blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, blockListElementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockListElementTypeName);
  await umbracoUi.content.clickAddBlockWithNameButton(textStringElementTypeName);
  await umbracoUi.content.clickLinkWithName(textStringElementTypeName, true);
  await umbracoUi.content.enterTextstring(textStringValue);
  await umbracoUi.content.clickCreateForModalWithHeadline('Add ' + textStringElementTypeName);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  // Asserts that the value in the BlockList is as expected
  const documentData = await umbracoApi.document.getByName(contentName);
  const documentValues = documentData.values.find(value => value.alias === AliasHelper.toAlias(blockListDataTypeName));
  expect(documentValues.value.contentData[0].values[0].value.contentData[0].values[0].value).toContain(textStringValue);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(secondBlockListDataTypeName);
});
