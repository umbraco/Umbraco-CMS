import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content Name
const contentName = 'ContentName';

// Document Type
const documentTypeName = 'DocumentTypeName';
let documentTypeId = null;
const documentTypeGroupName = 'DocumentGroup';

// Block Grid
const blockGridDataTypeName = 'BlockGridName';
let blockGridDataTypeId = null;

// Text String
const textStringElementTypeName = 'TextStringElementName';
let textStringElementTypeId = null;
let textStringGroupName = 'TextGroup';
const textStringDataTypeName = 'Textstring';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
});

test('can publish a block grid editor with a rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const richTextEditorValue = 'Hello World';
  const expectedRichTextEditorOutputValue = '<p>Hello World</p>';
  const richTextDataTypeName = 'RichTextDataTypeName';
  const richTextElementTypeName = 'RichTextElementName';
  const richTextElementGroupName = 'RichTextElementGroupName';
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextElementTypeName);

  const richTextEditorDataTypeId = await umbracoApi.dataType.createDefaultTiptapDataType(richTextDataTypeName);
  const richTextElementTypeId = await umbracoApi.documentType.createDefaultElementType(richTextElementTypeName, richTextElementGroupName, richTextDataTypeName, richTextEditorDataTypeId);
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, richTextElementTypeId, true);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId, documentTypeGroupName);
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
  const documentValues = documentData.values.find(value => value.alias === AliasHelper.toAlias(blockGridDataTypeName));
  expect(documentValues.value.contentData[0].values[0].value.markup).toContain(expectedRichTextEditorOutputValue);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextElementTypeName);
});

test('can publish a block grid editor with a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringValue = 'Hello World';
  const blockListDataTypeName = 'BlockListName';
  const blockListElementTypeName = 'BlockListElementName';
  const blockListElementGroupName = 'BlockListElementGroupName';
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);

  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);
  const blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, textStringElementTypeId);
  const blockListElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockListElementTypeName, blockListElementGroupName, blockListDataTypeName, blockListDataTypeId);
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, blockListElementTypeId, true);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId, documentTypeGroupName);
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
  const documentValues = documentData.values.find(value => value.alias === AliasHelper.toAlias(blockGridDataTypeName));
  expect(documentValues.value.contentData[0].values[0].value.contentData[0].values[0].value).toContain(textStringValue);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
});

test('can publish a block grid editor with a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringValue = 'Hello World';
  const secondBlockGridDataTypeName = 'SecondBlockGridDataTypeName';
  const blockGridElementTypeName = 'BlockGridElementTypeName';
  const blockGridElementGroupName = 'BlockGridElementGroupName';
  await umbracoApi.dataType.ensureNameNotExists(secondBlockGridDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);

  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);
  const secondBlockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(secondBlockGridDataTypeName, textStringElementTypeId);
  const blockGridElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockGridElementTypeName, blockGridElementGroupName, secondBlockGridDataTypeName, secondBlockGridDataTypeId);
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridDataTypeName, blockGridElementTypeId, true);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId, documentTypeGroupName);
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
  const documentValues = documentData.values.find(value => value.alias === AliasHelper.toAlias(blockGridDataTypeName));
  expect(documentValues.value.contentData[0].values[0].value.contentData[0].values[0].value).toContain(textStringValue);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(secondBlockGridDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
});
