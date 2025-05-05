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

const blockListDataTypeName = 'BlockListName';
let blockListDataTypeId = null;


// Element Types
const blockGridElementTypeName = 'BlockGridElementName';
let blockGridElementTypeId = null;
const blockGridElementGroupName = 'GridElementGroup';

const blockListElementTypeName = 'BlockListElementName';
let blockListElementTypeId = null;
const blockListElementGroupName = 'ListElementGroup';

const richTextElementTypeName = 'RichTextElementName';
let richTextElementTypeId = null;
const richTextElementGroupName = 'RTEElementGroup';


const textStringElementTypeName = 'TextStringElementName';
let textStringElementTypeId = null;
let textStringGroupName = 'TextGroup';
const textStringDataTypeName = 'Textstring';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
});

test('can publish a block grid editor with a rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const richTextDataTypeName = 'RichTextName';
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
  const richTextEditorValue = 'Hello World';
  const expectedRichTextEditorOutputValue = '<p>Hello World</p>';
  const richTextEditorDataTypeId = await umbracoApi.dataType.createEmptyRichTextEditor(richTextDataTypeName);
  richTextElementTypeId = await umbracoApi.documentType.createDefaultElementType(richTextElementTypeName, richTextElementGroupName, richTextDataTypeName, richTextEditorDataTypeId);
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
});

test('can publish a block grid editor with a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName)
  const textStringValue = 'Hello World';
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);
  blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, textStringElementTypeId);
  blockListElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockListElementTypeName, blockListElementGroupName, blockListDataTypeName, blockListDataTypeId);
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
});

test('can publish a block grid editor with a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringValue = 'Hello World';
  const secondBlockGridDataTypeName = 'SecondBlockGridDataTypeName';
  await umbracoApi.dataType.ensureNameNotExists(secondBlockGridDataTypeName);
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createDefaultElementType(textStringElementTypeName, textStringGroupName, textStringDataTypeName, textStringDataType.id);
  const secondBlockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(secondBlockGridDataTypeName, textStringElementTypeId);
  blockGridElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockGridElementTypeName, blockGridElementGroupName, secondBlockGridDataTypeName, secondBlockGridDataTypeId);
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
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
});

