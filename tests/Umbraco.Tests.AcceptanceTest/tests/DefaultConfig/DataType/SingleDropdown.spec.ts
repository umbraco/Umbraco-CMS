import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const propertyEditorName = 'Single Select Dropdown';
const customDataTypeName = 'Custom Single Dropdown';
const editorAlias = 'Umbraco.DropDown.Single';
const editorUiAlias = 'Umb.PropertyEditorUi.Dropdown.Single';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can create a single dropdown data type', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuForName('Data Types');
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(customDataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(propertyEditorName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  await umbracoUi.dataType.isDataTypeTreeItemVisible(customDataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(customDataTypeName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataTypeData.editorAlias).toBe(editorAlias);
  expect(dataTypeData.editorUiAlias).toBe(editorUiAlias);
});

test('the settings of a single dropdown are correct', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createSingleDropdownDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.singleDropdownSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.singleDropdownSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
});

test('fills in the value from the label of an added option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createSingleDropdownDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Test option');

  // Assert
  await umbracoUi.dataType.doesLastOptionHaveValueAndLabel('Test option', 'Test option');
});

test('stops filling in the value once it has been edited', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createSingleDropdownDataType(customDataTypeName);
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
  await umbracoApi.dataType.createSingleDropdownDataType(customDataTypeName, [{value: 'test-option', label: 'Test option'}]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterOptionLabel('Updated label', 0);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataTypeName, 'test-option', 'Updated label')).toBeTruthy();
});

test('cannot save an option without a label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createSingleDropdownDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionValue('Test option');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesOptionFieldShowError(0, 'label');
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataTypeName, 'Test option')).toBeFalsy();
});

test('cannot save an option without a value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createSingleDropdownDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Test option');
  await umbracoUi.dataType.enterOptionValue('');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesOptionFieldShowError(0, 'value');
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataTypeName, 'Test option')).toBeFalsy();
});

test('cannot save two options with the same value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createSingleDropdownDataType(customDataTypeName, [{value: 'Test option', label: 'Test option'}]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Test option');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isValidationMessageVisible(ConstantHelper.validationMessages.valueNotUnique);
  const dataType = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataType.values.find(item => item.alias === 'items').value).toHaveLength(1);
});
