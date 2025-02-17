import {expect} from '@playwright/test';
import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';

// DocumentType
const documentTypeName = 'TestDocumentType';
let documentTypeId = '';
const groupName = 'TestGroup';

// Content
const contentName = 'TestContent';
let contentId = '';

// Property Value
const wrongPropertyValue = 'This is a test with wrong value**';
const correctPropertyValue = 'Test';

// ElementTypes
// TextString Element Type (for Block List)
const textStringElementGroupName = 'TextStringElementGroup';
const textStringElementTypeName = 'TestElementWithTextString';
const textStringElementRegex = '^[a-zA-Z0-9]*$';
let textStringElementTypeId = '';
// Area Element Type (for Block Grid)
const areaElementTypeName = 'TestElementArea';
const areaAlias = 'testArea';
let areaElementTypeId = '';
// Rich Text Editor Element Type (for Block Grid)
const richTextEditorElementGroupName = 'RichTextEditorElementGroup';
const richTextEditorElementTypeName = 'RichTextEditorTestElement';
let richTextEditorElementTypeId = '';
// Block List Element Type
const blockListElementTypeName = 'BlockListElement';
const blockListGroupName = 'BlockListGroup';
let blockListElementTypeId = '';

// DataTypes
const blockGridDataTypeName = 'TestBlockGridEditor';
const blockListDataTypeName = 'TestBlockListEditor';
const textStringElementDataTypeName = 'Textstring';
const richTextDataTypeName = 'Rich Text Editor';
let blockListDataTypeId = '';
let blockGridDataTypeId = '';
test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(areaElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextEditorElementTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(textStringElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(areaElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextEditorElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
});

test('can update property value nested in a block grid area with an RTE with a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // ElementType with Textstring And REGEX only accept letters and numbers
  const textStringElementDataType = await umbracoApi.dataType.getByName(textStringElementDataTypeName);
  textStringElementTypeId = await umbracoApi.documentType.createElementTypeWithRegexValidation(textStringElementTypeName, textStringElementGroupName, textStringElementDataTypeName, textStringElementDataType.id, textStringElementRegex);
  // Block List Editor with Textstring
  blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, textStringElementTypeId);
  await umbracoUi.goToBackOffice();
  // ElementType with Block List Editor
  blockListElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockListElementTypeName, blockListGroupName, blockListDataTypeName, blockListDataTypeId);
  // Rich Text Editor in an ElementType,  with a Block(Element Type), the block contains a Block List Editor
  const richTextEditorId = await umbracoApi.dataType.createRichTextEditorWithABlock(richTextDataTypeName, blockListElementTypeId);
  richTextEditorElementTypeId = await umbracoApi.documentType.createDefaultElementType(richTextEditorElementTypeName, richTextEditorElementGroupName, richTextDataTypeName, richTextEditorId);
  // ElementType Area that is Empty
  areaElementTypeId = await umbracoApi.documentType.createEmptyElementType(areaElementTypeName);
  // Block Grid with 2 blocks, one with RTE and Inline, and one with areas
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockWithInlineEditingModeAndABlockWithAnArea(blockGridDataTypeName, richTextEditorElementTypeId, true, areaElementTypeId, areaAlias);
  // Document Type with the following
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId, groupName);
  // Creates Content
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockGridElementWithName(areaElementTypeName);
  await umbracoUi.content.clickSelectBlockElementWithName(areaElementTypeName);
  await umbracoUi.content.clickAddBlockGridElementWithName(richTextEditorElementTypeName);
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(richTextEditorElementTypeName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(blockListElementTypeName);
  await umbracoUi.content.clickAddBlockGridElementWithName(textStringElementTypeName);
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(textStringElementTypeName);
  // Enter text in the textstring block that wont match regex
  await umbracoUi.content.enterPropertyValue(textStringElementDataTypeName, wrongPropertyValue);
  await umbracoUi.content.clickCreateButtonForModalWithElementTypeNameAndGroupName(textStringElementTypeName, textStringElementGroupName);
  await umbracoUi.content.clickCreateButtonForModalWithElementTypeNameAndGroupName(blockListElementTypeName, blockListGroupName);
  await umbracoUi.content.clickCreateButtonForModalWithElementTypeNameAndGroupName(richTextEditorElementTypeName, richTextEditorElementGroupName);
  await umbracoUi.content.clickSaveAndPublishButton();
  // Checks that the error notification is shown since the textstring block has the wrong value
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved, true, true);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentWasNotPublished, true, true);
  // Updates the textstring block with the correct value
  await umbracoUi.content.clickBlockElementWithName(blockListElementTypeName);
  await umbracoUi.content.clickEditBlockListEntryWithName(textStringElementTypeName);
  await umbracoUi.content.enterPropertyValue(textStringElementDataTypeName, correctPropertyValue);
  await umbracoUi.content.clickUpdateButtonForModalWithElementTypeNameAndGroupName(textStringElementTypeName, textStringElementGroupName);
  await umbracoUi.content.clickUpdateButtonForModalWithElementTypeNameAndGroupName(blockListElementTypeName, blockListGroupName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved, true, true);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published, true, true);
  // Checks if published
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  // Checks if the textstring block has the correct value after reloading the page
  await umbracoUi.reloadPage();
  await umbracoUi.content.clickBlockElementWithName(blockListElementTypeName);
  await umbracoUi.content.clickEditBlockListEntryWithName(textStringElementTypeName);
  await umbracoUi.content.doesPropertyContainValue(textStringElementDataTypeName, correctPropertyValue);
});
