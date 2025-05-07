import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const customDataTypeName = 'Custom Dropdown';
const editorAlias = 'Umbraco.DropDown.Flexible';
const editorUiAlias = 'Umb.PropertyEditorUi.Dropdown';
const dropdowns = [
  {type: 'Dropdown', multipleChoice: false},
  {type: 'Dropdown multiple', multipleChoice: true}
];

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can enable multiple choice', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultDropdownDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickEnableMultipleChoiceToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'multiple', true)).toBeTruthy();
});

test('can add option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const optionName = 'Test option';
  await umbracoApi.dataType.createDefaultDropdownDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionName(optionName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'items', [optionName])).toBeTruthy();
});

test('can remove option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const removedOptionName = 'Removed Option';
  await umbracoApi.dataType.createDropdownDataType(customDataTypeName, false, [removedOptionName]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeOptionByName(removedOptionName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'items', [removedOptionName])).toBeFalsy();
});

for (const dropdown of dropdowns) {
  test(`the default configuration of ${dropdown.type} is correct`, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.goToDataType(dropdown.type);

    // Assert
    await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.dropdownSettings);
    await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.dropdownSettings);
    await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
    await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
    const dataTypeDefaultData = await umbracoApi.dataType.getByName(dropdown.type);
    expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
    expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(dropdown.type, 'multiple', dropdown.multipleChoice)).toBeTruthy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(dropdown.type, 'items')).toBeFalsy();
  });
}