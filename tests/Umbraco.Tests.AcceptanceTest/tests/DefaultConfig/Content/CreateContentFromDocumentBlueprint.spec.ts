import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestContentFromBlueprint';
const textContent = 'This is a block in document blueprint';

// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
const groupName = 'TestGroup';

// Document Blueprint
const documentBlueprintName = 'TestDocumentBlueprint';
const documentBlueprintDanishName = 'TestDocumentBlueprint-DA';

// ElementType
const elementGroupName = 'ElementGroup';
const elementTypeName = 'TestElement';
const richTextDataTypeUiAlias = 'Richtext editor';
const elementDataTypeUiAlias = 'Umbraco.RichText';
const elementPropertyName = 'TipTapProperty'

// DataType
const blockDataTypeName = 'TestBlockEditor';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockDataTypeName);
});

test('can create content using an invariant document blueprint', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'Textstring';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.documentBlueprint.createDocumentBlueprintWithTextBoxValue(documentBlueprintName, documentTypeId, dataTypeName, textContent);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.selectDocumentBlueprintWithName(documentBlueprintName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(documentBlueprintName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(documentBlueprintName);
  expect(contentData.values[0].value).toBe(textContent);
});

test('can create content using a variant document blueprint', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'Textstring';
  await umbracoApi.language.createDanishLanguage();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.documentBlueprint.createDocumenBlueprintWithEnglishCultureAndDanishCultureAndTextBoxValue(documentBlueprintName, documentBlueprintDanishName, documentTypeId, dataTypeName, textContent);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.selectDocumentBlueprintWithName(documentBlueprintName);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(documentBlueprintName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(documentBlueprintName);
  expect(contentData.values[0].value).toBe(textContent);
  expect(contentData.variants[0].name).toBe(documentBlueprintName);
  expect(contentData.variants[1].name).toBe(documentBlueprintDanishName);

  // Clean
  await umbracoApi.language.ensureNameNotExists('Danish');
});

test('can create content with different name using an invariant document blueprint', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'Textstring';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.documentBlueprint.createDocumentBlueprintWithTextBoxValue(documentBlueprintName, documentTypeId, dataTypeName, textContent);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.selectDocumentBlueprintWithName(documentBlueprintName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(documentBlueprintName)).toBeFalsy();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value).toBe(textContent);
});

test('can create content with different name using a variant document blueprint', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'Textstring';
  await umbracoApi.language.createDanishLanguage();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.documentBlueprint.createDocumenBlueprintWithEnglishCultureAndDanishCultureAndTextBoxValue(documentBlueprintName, documentBlueprintDanishName, documentTypeId, dataTypeName, textContent);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.selectDocumentBlueprintWithName(documentBlueprintName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value).toBe(textContent);
  expect(contentData.variants[0].name).toBe(contentName);
  expect(contentData.variants[1].name).toBe(documentBlueprintDanishName);

  // Clean
  await umbracoApi.language.ensureNameNotExists('Danish');
});

test('can create content using a document blueprint with block list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.ensureNameNotExists(blockDataTypeName);
  const tipTapDataType = await umbracoApi.dataType.getByName(richTextDataTypeUiAlias);
  const tipTapDataTypeId = tipTapDataType.id;
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, elementGroupName, elementPropertyName, tipTapDataTypeId);
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprintWithABlockListEditorAndBlockWithValue(documentBlueprintName, documentTypeName, blockDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), elementDataTypeUiAlias, textContent, groupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.selectDocumentBlueprintWithName(documentBlueprintName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(documentBlueprintName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(documentBlueprintName);
  expect(contentData.values[0].value.contentData[0].values[0].value.markup).toEqual(textContent);
  const blockListValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockList")?.value;
  expect(blockListValue).toBeTruthy();
});

test('can create content using a document blueprint with block grid', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.ensureNameNotExists(blockDataTypeName);
  const tipTapDataType = await umbracoApi.dataType.getByName(richTextDataTypeUiAlias);
  const tipTapDataTypeId = tipTapDataType.id;
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, elementGroupName, elementPropertyName, tipTapDataTypeId);
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprintWithABlockGridEditorAndBlockWithValue(documentBlueprintName, documentTypeName, blockDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), richTextDataTypeUiAlias, textContent);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.selectDocumentBlueprintWithName(documentBlueprintName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(documentBlueprintName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(documentBlueprintName);
  expect(contentData.values[0].value.contentData[0].values[0].value.markup).toEqual(textContent);
  const blockListValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockGrid")?.value;
  expect(blockListValue).toBeTruthy();
});
