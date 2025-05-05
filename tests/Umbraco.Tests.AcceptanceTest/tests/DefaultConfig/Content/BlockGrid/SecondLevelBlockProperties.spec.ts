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

const richTextDataTypeName = 'RichTextName';
let richTextDataTypeId = null;

// Element Types
const blockGridElementTypeName = 'BlockGridElementName';
let blockGridElementTypeId = null;
const blockGridElementGroupName = 'ElementGroup';

const blockListElementTypeName = 'BlockListElementName';
let blockListElementTypeId = null;
const blockListElementGroupName = 'ElementGroup';

const richTextElementTypeName = 'RichTextElementName';
let richTextElementTypeId = null;
const richTextElementGroupName = 'ElementGroup';

//
//
// // Property Editor
// const propertyEditorName = 'ProperyEditorInBlockName';
// let propertyEditorId = null;
// const optionValues = ['testOption1', 'testOption2'];

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(richTextElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  // await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  // await umbracoApi.dataType.ensureNameNotExists(richTextDataTypeName);
});

test('can publish a block grid editor with a rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
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
  await umbracoApi.document.doesBlockGridContainBlockWithRichTextEditorWithValue(contentName, AliasHelper.toAlias(blockGridDataTypeName));

  // Asserts that the value in the RTE is as expected
  const documentData = await umbracoApi.document.getByName(contentName);
  const documentValues = documentData.values.find(value => value.alias === AliasHelper.toAlias(blockGridDataTypeName));
  expect(documentValues.value.contentData[0].values[0].value.markup).toContain(expectedRichTextEditorOutputValue);
});

test('can publish a block grid editor with a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  propertyEditorId = await umbracoApi.dataType.createRadioboxDataType(propertyEditorName, optionValues);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockName, elementGroupName, propertyEditorName, propertyEditorId, true);
  blockGridId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridName, elementTypeId, true);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridName, blockGridId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  // Do not select any radiobox values and the validation error appears
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  // Select a radiobox value and the validation error disappears
  await umbracoUi.content.chooseRadioboxOption(optionValues[0]);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue, false);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
});

test('can publish a block grid editor with a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  propertyEditorId = await umbracoApi.dataType.createRadioboxDataType(propertyEditorName, optionValues);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockName, elementGroupName, propertyEditorName, propertyEditorId, true);
  blockGridId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridName, elementTypeId, true);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridName, blockGridId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  // Do not select any radiobox values and the validation error appears
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  // Select a radiobox value and the validation error disappears
  await umbracoUi.content.chooseRadioboxOption(optionValues[0]);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue, false);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
});

