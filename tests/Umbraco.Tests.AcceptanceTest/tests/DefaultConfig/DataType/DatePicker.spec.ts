import { ConstantHelper, test } from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

const editorAlias = 'Umbraco.DateTime';
const editorUiAlias = 'Umb.PropertyEditorUi.DatePicker';
const datePickerTypes = ['Date Picker', 'Date Picker with time'];
for (const datePickerType of datePickerTypes) {
  test.describe(`${datePickerType} tests`, () => {
    let dataTypeDefaultData = null;
    let dataTypeData = null;

    test.beforeEach(async ({ umbracoUi, umbracoApi }) => {
      await umbracoUi.goToBackOffice();
      await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
      dataTypeDefaultData = await umbracoApi.dataType.getByName(datePickerType);
      await umbracoUi.dataType.goToDataType(datePickerType);
    });

    test.afterEach(async ({ umbracoApi }) => {
      if (dataTypeDefaultData !== null) {
        await umbracoApi.dataType.update(dataTypeDefaultData.id,dataTypeDefaultData);
      }
    });

    // This test is out-of-date since currently it is impossible to update offset time in front-end
    test.skip(`can update offset time`, async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const expectedDataTypeValues =
        datePickerType === 'Date Picker'
          ? [
              {
                "alias": "format",
                "value": "YYYY-MM-DD",
              },
              {
                "alias": "offsetTime",
                "value": true,
              },
            ]
          : [
              {
                "alias": "format",
                "value": "YYYY-MM-DD HH:mm:ss",
              },
              {
                "alias": "offsetTime",
                "value": true,
              }
            ];

      // Act
      await umbracoUi.dataType.clickOffsetTimeToggle();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(datePickerType);
      expect(dataTypeData.values).toEqual(expectedDataTypeValues);
    });

    test('can update date format', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dateFormatValue =
        datePickerType === "Date Picker" ? "DD-MM-YYYY" : "DD-MM-YYYY hh:mm:ss";
      const expectedDataTypeValues = {
        "alias": "format",
        "value": dateFormatValue
      };
      // Act
      await umbracoUi.dataType.enterDateFormatValue(dateFormatValue);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(datePickerType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('the default configuration is correct', async ({umbracoUi}) => {
      // Arrange
      const defaultDateFormatValue =
        datePickerType === "Date Picker" ? "YYYY-MM-DD" : "YYYY-MM-DD HH:mm:ss";

      // Assert
      await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.datePickerSettings);
      await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.datePickerSettings);
      await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
      await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
      expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
      expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
      expect(dataTypeDefaultData.values[0].value).toEqual(defaultDateFormatValue);
    });
  });
}
