import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content Name
const contentName = 'ContentName';

// Document Type
const documentTypeName = 'DocumentTypeName';
let documentTypeId = null;
const documentTypeGroupName = 'DocumentGroup';

// Rich Text Editor
const richTextDataTypeName = 'RichTextDataType';
let richTextDataTypeId = null;

// Text String
const textStringElementTypeName = 'TextStringElementName';
let textStringElementTypeId = null;
let textStringGroupName = 'TextGroup';
const textStringDataTypeName = 'Textstring';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
});

test('can publish a rich text editor with a rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const richTextEditorValue = 'Hello First World';
  const secondRichTextEditorValue = 'Hello Second World';
  const expectedRichTextEditorOutputValue = '<p>' + richTextEditorValue + '</p>';
  const secondExpectedRichTextEditorOutputValue = '<p>' + secondRichTextEditorValue + '</p>';
  const secondRichTextDataTypeName = 'SecondRichTextName';
  const richTextElementTypeName = 'RichTextElementName';
  const richTextElementGroupName = 'RichTextElementGroupName';
  await umbracoApi.dataType.ensureNameNotExists(secondRichTextDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextElementGroupName);

  const secondRichTextEditorDataTypeId = await umbracoApi.dataType.createDefaultTiptapDataType(secondRichTextDataTypeName);
  const richTextElementTypeId = await umbracoApi.documentType.createDefaultElementType(richTextElementTypeName, richTextElementGroupName, secondRichTextDataTypeName, secondRichTextEditorDataTypeId);
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
  await umbracoApi.documentType.ensureNameNotExists(richTextElementGroupName);
});

test('can publish a rich text editor with a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringValue = 'Hello World';
  const blockGridDataTypeName = 'BlockGridDataTypeName';
  const blockGridElementTypeName = 'BlockGridElementTypeName';
  const blockGridElementGroupName = 'BlockGridElementGroupName';
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);

  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);
  const blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, textStringElementTypeId);
  const blockGridElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockGridElementTypeName, blockGridElementGroupName, blockGridDataTypeName, blockGridDataTypeId);
  richTextDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlock(richTextDataTypeName, blockGridElementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, richTextDataTypeName, richTextDataTypeId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInsertBlockButton();
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
  expect(documentData.values[0].value.blocks.contentData[0].values[0].value.contentData[0].values[0].value).toContain(textStringValue);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
});

test('can publish a rich text editor with a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringValue = 'Hello World';
  const blockListDataTypeName = 'BlockListName';
  const blockListElementTypeName = 'BlockListElementName';
  const blockListElementGroupName = 'BlockListGroupName';
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);

  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);
  const blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, textStringElementTypeId);
  const blockListElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockListElementTypeName, blockListElementGroupName, blockListDataTypeName, blockListDataTypeId);
  richTextDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlock(richTextDataTypeName, blockListElementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, richTextDataTypeName, richTextDataTypeId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickLinkWithName(blockListElementTypeName, true);
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
  expect(documentData.values[0].value.blocks.contentData[0].values[0].value.contentData[0].values[0].value).toContain(textStringValue);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
});
