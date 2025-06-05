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

test('can not publish a block list with a mandatory radiobox without a value', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
});

test('can not publish a block list with a mandatory checkbox list without a value', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
});

test('can not publish a block list with a mandatory dropdown without a value', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
});
