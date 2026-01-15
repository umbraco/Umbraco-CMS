import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Content
const contentName = 'TestContent';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// DataType
const dataTypeName = 'CustomTextBox';
const editorUiAlias = 'Custom.TextEditor';
const editorAlias = 'Umbraco.TextBox';
// Property Editor
const customPropertyEditorName = 'Custom Text Editor';
// Test values
const testValue = 'This is a test value for the custom property editor';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can add custom property editor to a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(customPropertyEditorName);
  const dataTypeId = await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  expect(await umbracoApi.dataType.doesExist(dataTypeId)).toBe(true);
  await umbracoUi.dataType.isDataTypeTreeItemVisible(dataTypeName);
});

test('can select custom property editor in property editor picker on data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  const dataTypeId = await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias, []);
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickAddGroupButton();
  await umbracoUi.documentType.addPropertyEditor(dataTypeName);
  await umbracoUi.documentType.enterGroupName('Content');
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  // Checks if the correct property was added to the document type
  expect(documentTypeData.properties[0].dataType.id).toBe(dataTypeId);
});

test('can write and read value from custom property editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeValue = [
    {
      alias: "maxChars",
      value: "100",
    },
  ];
  const dataTypeId = await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias, dataTypeValue);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, "Test content", dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterPropertyValue(dataTypeName, testValue);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value).toEqual(testValue);
});
