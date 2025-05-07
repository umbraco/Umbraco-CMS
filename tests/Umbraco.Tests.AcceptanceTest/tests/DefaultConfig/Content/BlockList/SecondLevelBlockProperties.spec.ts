import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content Name
const contentName = 'ContentName';

// Document Type
const documentTypeName = 'DocumentTypeName';
let documentTypeId = null;
const documentTypeGroupName = 'DocumentGroup';

// Block List
const blockListDataTypeName = 'BlockListName';
let blockListDataTypeId = null;

// Text String
const textStringElementTypeName = 'TextStringElementName';
let textStringElementTypeId = null;
let textStringGroupName = 'TextGroup';
const textStringDataTypeName = 'Textstring';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
});

test('can publish a block list editor with a rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const richTextEditorValue = 'Hello World';
  const expectedRichTextEditorOutputValue = '<p>Hello World</p>';
  const richTextDataTypeName = 'RichTextName';
  const richTextElementTypeName = 'RichTextElementName';
  const richTextElementGroupName = 'RTEElementGroup';
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextElementTypeName);

  const richTextEditorDataTypeId = await umbracoApi.dataType.createDefaultTiptapDataType(richTextDataTypeName);
  const richTextElementTypeId = await umbracoApi.documentType.createDefaultElementType(richTextElementTypeName, richTextElementGroupName, richTextDataTypeName, richTextEditorDataTypeId);
  blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, richTextElementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(richTextElementTypeName);
  await umbracoUi.content.enterRTETipTapEditor(richTextEditorValue);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  // Asserts that the value in the RTE is as expected
  const documentData = await umbracoApi.document.getByName(contentName);
  const documentRichTextValues = documentData.values[0].value.contentData[0].values.find(value => value.alias === AliasHelper.toAlias(richTextDataTypeName));
  expect(documentRichTextValues.value.markup).toContain(expectedRichTextEditorOutputValue);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextElementTypeName);
});

test('can publish a block list editor with a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringValue = 'Hello World';
  const blockGridDataTypeName = 'BlockGridDataTypeName';
  const blockGridElementTypeName = 'BlockGridElementName';
  const blockGridElementGroupName = 'GridElementGroup';
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeId);

  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);
  const blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, textStringElementTypeId);
  const blockGridElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockGridElementTypeName, blockGridElementGroupName, blockGridDataTypeName, blockGridDataTypeId);
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
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeId);
});

test('can publish a block list editor with a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringValue = 'Hello World';
  const secondBlockListDataTypeName = 'SecondBlockListName';
  const blockListElementTypeName = 'BlockListElementName';
  const blockListElementGroupName = 'ListElementGroup';
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);

  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);
  const secondBlockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(secondBlockListDataTypeName, textStringElementTypeId);
  const blockListElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockListElementTypeName, blockListElementGroupName, secondBlockListDataTypeName, secondBlockListDataTypeId);
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
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
});
