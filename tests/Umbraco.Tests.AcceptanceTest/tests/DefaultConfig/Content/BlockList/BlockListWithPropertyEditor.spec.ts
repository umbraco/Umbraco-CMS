import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content Name
const contentName = 'ContentName';
let contentId = null;

// Document Type
const documentTypeName = 'DocumentTypeName';
let documentTypeId = null;
const documentTypeGroupName = 'DocumentGroup';

// Block List
const blockListName = 'BlockListName';
let blockListId = null;

// Element Type
const blockName = 'BlockName';
let elementTypeId = null;
const elementGroupName = 'ElementGroup';

// Property Editor
const propertyEditorName = 'ProperyEditorInBlockName';
let propertyEditorId = null;
const optionValues = ['testOption1', 'testOption2'];

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
});

test('cannot publish a block list with a mandatory radiobox without a value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  propertyEditorId = await umbracoApi.dataType.createRadioboxDataType(propertyEditorName, optionValues);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockName, elementGroupName, propertyEditorName, propertyEditorId, true);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

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
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
});

test('cannot publish a block list with a mandatory checkbox list without a value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  propertyEditorId = await umbracoApi.dataType.createCheckboxListDataType(propertyEditorName, optionValues);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockName, elementGroupName, propertyEditorName, propertyEditorId, true);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  // Do not select any checkbox list values and the validation error appears
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  // Select a checkbox list value and the validation error disappears
  await umbracoUi.content.chooseCheckboxListOption(optionValues[0]);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue, false);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
});

test('cannot publish a block list with a mandatory dropdown without a value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  propertyEditorId = await umbracoApi.dataType.createDropdownDataType(propertyEditorName, false, optionValues);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockName, elementGroupName, propertyEditorName, propertyEditorId, true);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  // Do not select any dropdown values and the validation error appears
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  // Select a dropdown value and the validation error disappears
  await umbracoUi.content.chooseDropdownOption([optionValues[0]]);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue, false);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
});

test('cannot update a variant block list with invalid text', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringElementDataTypeName = 'Textstring';
  const textStringElementRegex = '[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+';
  const wrongPropertyValue = 'This is an invalid email';
  const correctPropertyValue = 'validemail@test.com';
  // Create a new language
  await umbracoApi.language.createDanishLanguage();
  // ElementType with textstring and regex only accept an email address
  const textStringElementDataType = await umbracoApi.dataType.getByName(textStringElementDataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithRegexValidation(blockName, elementGroupName, textStringElementDataTypeName, textStringElementDataType.id, textStringElementRegex);
  // Block List Editor with textstring
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  // Document Type with Block List Editor
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName, true, true);
  // Creates content
  contentId = await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  // Enter text in the textstring block that won't match regex
  await umbracoUi.content.enterPropertyValue(textStringElementDataTypeName, wrongPropertyValue);
  await umbracoUi.content.clickCreateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveButton();
  // Verify that the block list entry has an invalid badge
  await umbracoUi.content.doesPropertyHaveInvalidBadge(blockListName);
  await umbracoUi.content.clickEditBlockListEntryWithName(blockName);
  await umbracoUi.content.doesModalFormValidationMessageContainText(ConstantHelper.validationMessages.invalidValue);
  // Update the textstring block with a valid email address
  await umbracoUi.content.enterPropertyValue(textStringElementDataTypeName, correctPropertyValue);
  await umbracoUi.content.clickUpdateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toContain(correctPropertyValue);
  const blockListValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockList")?.value;
  expect(blockListValue).toBeTruthy();
  await umbracoUi.content.clickEditBlockListEntryWithName(blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringElementDataTypeName, correctPropertyValue);
});