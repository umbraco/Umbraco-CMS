import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentType';
const elementName = 'TestElement';
const elementDataTypeName = 'Textstring';
const elementDataTypeUiAlias = 'Umbraco.Textbox';
const blockListName = 'TestBlockListEditor';
const propertyName = 'TextStringProperty'
const propertyValue = 'This is a test';
let textStringDataTypeId = '';
let elementId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
  const textStringDataType = await umbracoApi.dataType.getByName(elementDataTypeName);
  textStringDataTypeId = textStringDataType.id;
  elementId = await umbracoApi.documentType.createDefaultElementType(elementName, 'testGroup', propertyName, textStringDataTypeId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
});

test('can copy a single block', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithValue(contentName, documentTypeName, blockListName, elementId, AliasHelper.toAlias(propertyName), propertyValue, elementDataTypeUiAlias);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await page.pause()
  await umbracoUi.content.goToBlockListBlockWithName(elementName);
  await page.pause()

});

test('can copy and paste a single block into the same document and group', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
});

test('can copy and paste a single block into the same document but different group', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
});

test('can copy and paste a single block into another document', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
});


test('can copy and paste multiple blocks into the same document and group', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
});

test('can copy and paste a multiple blocks into the same document but different group', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
});

test('can copy and paste a multiple blocks into another document', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
});
