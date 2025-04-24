import { ConstantHelper, NotificationConstantHelper, test } from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

const labelTypes = [
  {type: 'Label (bigint)', dataValueType: 'BIGINT'},
  {type: 'Label (datetime)', dataValueType: 'DATETIME'},
  {type: 'Label (decimal)', dataValueType: 'DECIMAL'},
  {type: 'Label (integer)', dataValueType: 'INT'},
  {type: 'Label (string)', dataValueType: 'STRING'},
  {type: 'Label (time)', dataValueType: 'TIME'}
];
const editorAlias = 'Umbraco.Label';
const editorUiAlias = 'Umb.PropertyEditorUi.Label';
const customDataTypeName = 'Custom Label';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can change value type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultLabelDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.changeValueType("Long String");
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'umbracoDataValueType', 'TEXT')).toBeTruthy();
});

for (const label of labelTypes) {
  test(`the default configuration of ${label.type} is correct`, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.goToDataType(label.type);

    // Assert
    await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.labelSettings);
    await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.labelSettings);
    await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
    await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(label.type, 'umbracoDataValueType', label.dataValueType)).toBeTruthy();
  });
}
