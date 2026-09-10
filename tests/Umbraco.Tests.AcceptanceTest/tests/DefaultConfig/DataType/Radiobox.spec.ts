import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Radiobox';
const editorAlias = 'Umbraco.RadioButtonList';
const editorUiAlias = 'Umb.PropertyEditorUi.RadioButtonList';
const customDataTypeName = 'Custom Radiobox';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can add option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const optionName = 'Test option';
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionName(optionName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'items', [optionName])).toBeTruthy();
});

test('can remove option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const removedOptionName = 'Removed Option';
  const customDataType = 'Custom Radiobox';
  await umbracoApi.dataType.createRadioboxDataType(customDataType, [removedOptionName]);
  await umbracoUi.dataType.goToDataType(customDataType);

  // Act
  await umbracoUi.dataType.removeOptionByName(removedOptionName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataType, 'items', [removedOptionName])).toBeFalsy();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataType);
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.radioboxSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.radioboxSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.dataType.doesDataTypeHaveEditors(dataTypeDefaultData, editorAlias, editorUiAlias);
  await umbracoApi.dataType.doesHaveValueCount(dataTypeDefaultData, 0);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'items')).toBeFalsy();
});
