import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentType';
const elementName = 'TestElement';
const elementDataTypeName = 'Textstring';
const elementDataTypeUiAlias = 'Umbraco.Textbox';
const blockGridName = 'TestBlockGridEditor';
const propertyName = 'TextStringProperty'
const propertyValue = 'This is a test';
let textStringDataTypeId = '';
let elementId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridName);
  const textStringDataType = await umbracoApi.dataType.getByName(elementDataTypeName);
  textStringDataTypeId = textStringDataType.id;
  elementId = await umbracoApi.documentType.createDefaultElementType(elementName, 'testGroup', propertyName, textStringDataTypeId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridName);
});

test('can copy a single block', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  // blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(blockGridName, elementId, true);
  // documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementName, blockGridDataTypeId);
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValue(contentName, documentTypeName, blockGridName, elementId, AliasHelper.toAlias(propertyName), propertyValue, elementDataTypeUiAlias);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await page.pause()

});

