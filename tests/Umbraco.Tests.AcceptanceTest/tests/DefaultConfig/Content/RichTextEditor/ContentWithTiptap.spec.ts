import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Test RTE Tiptap';
let customDataTypeId = null;

test.beforeEach(async ({umbracoApi}) => {
  customDataTypeId = await umbracoApi.dataType.createDefaultTiptapDataType(customDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can create content with empty RTE Tiptap property editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

test('can create content with non-empty RTE Tiptap property editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const inputText = 'Test Tiptap here';
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.enterRTETipTapEditor(inputText);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].value.markup).toEqual('<p>' + inputText + '</p>');
});

test('can publish content with RTE Tiptap property editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const inputText = 'Test Tiptap here';
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterRTETipTapEditor(inputText);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].value.markup).toEqual('<p>' + inputText + '</p>');
});

// This is a test for the regression issue #19763
test('can save a variant content node after removing embedded block in RTE', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Language
  const danishIsoCode = 'da';
  await umbracoApi.language.createDanishLanguage();
  // Content Names
  const englishContentName = 'English Content';
  const danishContentName = 'Danish Content';
  // Element Type
  const elementTypeName = 'Default Element Type';
  const elementTypeGroupName = 'Content';
  const elementTypeDataTypeName = 'Textstring';
  const elementTypeDataType = await umbracoApi.dataType.getByName(elementTypeDataTypeName);
  // Rich Text Editor
  const richTextEditorDataTypeName = 'Rich Text Editor with a block';
  const textStringValue = 'Block Content';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(elementTypeName, elementTypeGroupName, elementTypeDataTypeName, elementTypeDataType.id, true, false);
  const richTextEditorId = await umbracoApi.dataType.createRichTextEditorWithABlock(richTextEditorDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, richTextEditorDataTypeName, richTextEditorId, 'TestGroup', true, false);
  const cultures = [{isoCode: 'en-US', name: englishContentName}, {isoCode: 'da', name: danishContentName}];
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(contentName, documentTypeId, AliasHelper.toAlias(richTextEditorDataTypeName), 'Umbraco.RichText', cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(englishContentName);

  // Act
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(textStringValue);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeUpdated();
  const contentData = await umbracoApi.document.getByName(englishContentName);
  expect(contentData.values[0].value.blocks.contentData[0].values[0].value).toBe(textStringValue);
  await umbracoUi.content.clearTipTapEditor();
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeUpdated();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(englishContentName)).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(richTextEditorDataTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});
