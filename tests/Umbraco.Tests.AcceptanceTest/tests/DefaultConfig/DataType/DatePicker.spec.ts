import { ConstantHelper, NotificationConstantHelper, test } from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

const editorAlias = 'Umbraco.DateTime';
const editorUiAlias = 'Umb.PropertyEditorUi.DatePicker';
const datePickerTypes = [
  {type: 'Date Picker', format: 'YYYY-MM-DD'},
  {type: 'Date Picker with time', format: 'YYYY-MM-DD HH:mm:ss'}
];
const customDataTypeName = 'Custom DateTime';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can update date format', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dateFormatValue = 'DD-MM-YYYY hh:mm:ss';
  await umbracoApi.dataType.createDefaultDateTimeDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterDateFormatValue(dateFormatValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'format', dateFormatValue)).toBeTruthy();
});

for (const datePickerType of datePickerTypes) {
  test(`the default configuration of ${datePickerType.type} is correct`, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.goToDataType(datePickerType.type);

    // Assert
    await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.datePickerSettings);
    await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.datePickerSettings);
    await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
    await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(datePickerType.type, 'format', datePickerType.format)).toBeTruthy();
  });
}