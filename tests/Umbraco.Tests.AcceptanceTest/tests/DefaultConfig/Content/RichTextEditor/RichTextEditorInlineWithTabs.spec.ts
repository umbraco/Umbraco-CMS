import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Tests regression issue: https://github.com/umbraco/Umbraco-CMS/issues/20866
// Block in RTE no longer render properties when document type has tab structure
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom RTE With Block';
const elementTypeName = 'RTEBlockElement';
const dataTypeName = 'Textstring';
const rteTabName = 'RTETab';
const secondTabName = 'ContentTab';
const groupName = 'testGroup';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can render block properties in RTE when document type has tab structure', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlock(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName, customDataTypeName, customDataTypeId, rteTabName, secondTabName, dataTypeName, textStringData.id, groupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(rteTabName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);

  // Assert
  await umbracoUi.content.isTextstringPropertyVisible();
});

test('can save block in RTE when document type has tab structure', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'Test RTE block with tabs';
  const customDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlock(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName, customDataTypeName, customDataTypeId, rteTabName, secondTabName, dataTypeName, textStringData.id, groupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(rteTabName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.blocks.contentData[0].values[0].value).toEqual(inputText);
});

test('can render block properties in RTE when RTE is on second tab', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlock(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  // RTE is on second tab, textstring is on first tab
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTabAndAnotherTab(documentTypeName, dataTypeName, textStringData.id, rteTabName, secondTabName, customDataTypeName, customDataTypeId, groupName, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(secondTabName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);

  // Assert
  await umbracoUi.content.isTextstringPropertyVisible();
});

test('can render block properties in RTE when RTE is directly under tab without group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createRichTextEditorWithABlock(customDataTypeName, elementTypeId);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorDirectlyInTwoTabs(documentTypeName, customDataTypeName, customDataTypeId, rteTabName, secondTabName, dataTypeName, textStringData.id, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTabWithName(rteTabName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);

  // Assert
  await umbracoUi.content.isTextstringPropertyVisible();
});
