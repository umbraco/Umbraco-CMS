import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'TestDataType';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can create a data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor('Text Box');
  const dataTypeId = await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  expect(await umbracoApi.dataType.doesExist(dataTypeId)).toBe(true);
  await umbracoUi.dataType.isDataTypeTreeItemVisible(dataTypeName);
});

test('can rename a data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongDataTypeName = 'Wrong Data Type';
  await umbracoApi.dataType.ensureNameNotExists(wrongDataTypeName);
  await umbracoApi.dataType.createTextstringDataType(wrongDataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(wrongDataTypeName);
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  await umbracoUi.dataType.isDataTypeTreeItemVisible(dataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeName)).toBeFalsy();
});

test('can delete a data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createTextstringDataType(dataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.deleteDataTypeAndWaitForDataTypeToBeDeleted(dataTypeName);

  // Assert
  await umbracoUi.dataType.isDataTypeTreeItemVisible(dataTypeName, false);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeFalsy();
});

test('can change property editor in a data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedEditorName = 'Text Area';
  const updatedEditorAlias = 'Umbraco.TextArea';
  const updatedEditorUiAlias = 'Umb.PropertyEditorUi.TextArea';
  const maxChars = 500;

  await umbracoApi.dataType.createTextstringDataType(dataTypeName, maxChars);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);
  await umbracoUi.dataType.clickChangeButton();
  await umbracoUi.dataType.selectAPropertyEditor(updatedEditorName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.editorAlias).toBe(updatedEditorAlias);
  expect(dataTypeData.editorUiAlias).toBe(updatedEditorUiAlias);

  const maxCharsSetting = dataTypeData.values.find((x: {alias: string, value: unknown}) => x.alias === 'maxChars');
  expect(maxCharsSetting.value, 'Stored configuration should be transferred').toBe(maxChars);
});

test('cannot create a data type without selecting the property editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isFailedStateButtonVisible();
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeFalsy();
});

test('can change settings', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxCharsValue = 126;
  await umbracoApi.dataType.createTextstringDataType(dataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);
  await umbracoUi.dataType.enterMaximumAllowedCharactersValue(maxCharsValue.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'maxChars', maxCharsValue)).toBeTruthy();
});
