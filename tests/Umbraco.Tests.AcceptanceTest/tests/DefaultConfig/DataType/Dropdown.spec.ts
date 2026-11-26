import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const customDataTypeName = 'Custom Dropdown';
// There is one dropdown editor per number of values it holds, so the built-in data types are on
// different editors rather than differing by configuration.
const dropdowns = [
  {
    type: 'Single Select Dropdown',
    editorAlias: 'Umbraco.DropDown.Single',
    editorUiAlias: 'Umb.PropertyEditorUi.Dropdown.Single',
    settings: ConstantHelper.singleDropdownSettings
  },
  {
    type: 'Select List',
    editorAlias: 'Umbraco.DropDown.Flexible',
    editorUiAlias: 'Umb.PropertyEditorUi.Dropdown',
    settings: ConstantHelper.dropdownSettings
  }
];

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
  await umbracoApi.dataType.createDefaultDropdownDataType(customDataTypeName);
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
  await umbracoApi.dataType.createDropdownDataType(customDataTypeName, false, [{value: removedOptionName, label: removedOptionName}]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeOption(0);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataTypeName, removedOptionName)).toBeFalsy();
});

for (const dropdown of dropdowns) {
  test(`the default configuration of ${dropdown.type} is correct`, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.goToDataType(dropdown.type);

    // Assert
    await umbracoUi.dataType.doesSettingHaveValue(dropdown.settings);
    await umbracoUi.dataType.doesSettingItemsHaveCount(dropdown.settings);
    await umbracoUi.dataType.doesPropertyEditorHaveAlias(dropdown.editorAlias);
    await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(dropdown.editorUiAlias);
    const dataTypeDefaultData = await umbracoApi.dataType.getByName(dropdown.type);
    expect(dataTypeDefaultData.editorAlias).toBe(dropdown.editorAlias);
    expect(dataTypeDefaultData.editorUiAlias).toBe(dropdown.editorUiAlias);
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(dropdown.type, 'multiple')).toBeFalsy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(dropdown.type, 'items')).toBeFalsy();
  });
}

test('fills in the value from the label of an added option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultDropdownDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Test option');

  // Assert
  await umbracoUi.dataType.doesLastOptionHaveValueAndLabel('testOption', 'Test option');
});

test('stops filling in the value once it has been edited', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultDropdownDataType(customDataTypeName);
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
  await umbracoApi.dataType.createDropdownDataType(customDataTypeName, true, [{value: 'test-option', label: 'Test option'}]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterOptionLabel('Updated label', 0);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveOption(customDataTypeName, 'test-option', 'Updated label')).toBeTruthy();
});

test('cannot save an option without a label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultDropdownDataType(customDataTypeName);
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
  await umbracoApi.dataType.createDefaultDropdownDataType(customDataTypeName);
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
  await umbracoApi.dataType.createDropdownDataType(customDataTypeName, true, [{value: 'testOption', label: 'Test option'}]);
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

test('cannot save two options with an empty value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDropdownDataType(customDataTypeName, true, [{value: '', label: 'None'}]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionLabel('Empty');
  await umbracoUi.dataType.enterOptionValue('');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isValidationMessageVisible(ConstantHelper.validationMessages.valueNotUnique);
  const dataType = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataType.values.find(item => item.alias === 'items').value).toHaveLength(1);
});
