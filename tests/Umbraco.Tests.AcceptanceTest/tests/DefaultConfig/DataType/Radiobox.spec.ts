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
  const optionValue = 'testOption';
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel(optionName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataTypeName, optionValue, optionName)).toBeTruthy();
});

test('can remove option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const removedOptionName = 'Removed Option';
  const customDataType = 'Custom Radiobox';
  await umbracoApi.dataType.createRadioboxDataType(customDataType, [{value: removedOptionName, label: removedOptionName}]);
  await umbracoUi.dataType.goToDataType(customDataType);

  // Act
  await umbracoUi.dataType.removeOption(0);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataType, removedOptionName)).toBeFalsy();

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
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'items')).toBeFalsy();
});

test('fills in the value from the label of an added option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Test option');

  // Assert
  await umbracoUi.dataType.doesLastOptionHaveValueAndLabel('testOption', 'Test option');
});

test('stops filling in the value once it has been edited', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Test option');
  await umbracoUi.dataType.enterOptionValue('custom-value');
  await umbracoUi.dataType.enterOptionLabel('Edited label');

  // Assert
  await umbracoUi.dataType.doesLastOptionHaveValueAndLabel('custom-value', 'Edited label');
});

test('does not change the value of a saved option when its label is edited', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName, [{value: 'test-option', label: 'Test option'}]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterOptionLabel('Updated label', 0);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataTypeName, 'test-option', 'Updated label')).toBeTruthy();
});

test('cannot save an option without a label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionValue('Test option');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesOptionFieldShowError(0, 'label');
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataTypeName, 'Test option')).toBeFalsy();
});

test('can save an option with an empty value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('None');
  await umbracoUi.dataType.enterOptionValue('');
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataTypeName, '', 'None')).toBeTruthy();
});

test('cannot save two options with the same value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName, [{value: 'testOption', label: 'Test option'}]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Another option');
  await umbracoUi.dataType.enterOptionValue('testOption');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isValidationMessageVisible(ConstantHelper.validationMessages.duplicateFieldValue('Value', 'testOption'));
  const dataType = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataType.values.find(item => item.alias === 'items').value).toHaveLength(1);
});

test('cannot save two options with the same label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName, [{value: 'testOption', label: 'Test option'}]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Test option');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isValidationMessageVisible(ConstantHelper.validationMessages.duplicateFieldValue('Label', 'Test option'));
  const dataType = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataType.values.find(item => item.alias === 'items').value).toHaveLength(1);
});

test('cannot save two options with an empty value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createRadioboxDataType(customDataTypeName, [{value: '', label: 'None'}]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Empty');
  await umbracoUi.dataType.enterOptionValue('');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isValidationMessageVisible(ConstantHelper.validationMessages.duplicateFieldValue('Value', ''));
  const dataType = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataType.values.find(item => item.alias === 'items').value).toHaveLength(1);
});
