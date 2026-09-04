import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const customDataTypeName = 'Custom Dropdown';
// There is one dropdown editor per number of values it holds, so the built-in data types are on
// different editors rather than differing by configuration.
const dropdowns = [
  {
    type: 'Dropdown',
    editorAlias: 'Umbraco.SingleDropDown',
    editorUiAlias: 'Umb.PropertyEditorUi.SingleDropdown',
    settings: ConstantHelper.singleDropdownSettings
  },
  {
    type: 'Dropdown multiple',
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
  await umbracoApi.dataType.createDefaultDropdownDataType(customDataTypeName);
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
  await umbracoApi.dataType.createDropdownDataType(customDataTypeName, false, [removedOptionName]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeOptionByName(removedOptionName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'items', [removedOptionName])).toBeFalsy();
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
