import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Date Picker and Date Picker with time tests', () => {
  let dataTypeDefaultData = null;
  let dataTypeData = null;


  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  });

  test.afterEach(async ({umbracoApi}) => {
    if (dataTypeDefaultData !== null) {
      await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData); 
    }   
  });

  const datePickerTypes = ['Date Picker', 'Date Picker with time'];
  for (const datePickerType of datePickerTypes) {
    test(`can update offset time in ${datePickerType}`, async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const expectedDataTypeValues =
        datePickerType === "Date Picker"
          ? [
              {
                alias: "format",
                value: "YYYY-MM-DD",
              },
              {
                alias: "offsetTime",
                value: true,
              }
            ]
          : [
              {
                alias: "offsetTime",
                value: true,
              },
            ];
      dataTypeDefaultData = await umbracoApi.dataType.getByName(datePickerType);
      await umbracoUi.dataType.goToDataType(datePickerType);

      // Act
      await umbracoUi.dataType.clickOffsetTimeSlider();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      await umbracoUi.dataType.isSuccessNotificationVisible();
      dataTypeData = await umbracoApi.dataType.getByName(datePickerType);
      expect(dataTypeData.values).toEqual(expectedDataTypeValues);
    });

    test(`can update date format in ${datePickerType}`, async ({umbracoApi, umbracoUi}) => {  
      // Arrange
      const dateFormatValue = (datePickerType === 'Date Picker') ? 'DD-MM-YYYY' : 'DD-MM-YYYY hh:mm:ss';
      const expectedDataTypeValues =  [
        {
          "alias": "format",
          "value": dateFormatValue
        }
      ];
      
      dataTypeDefaultData = await umbracoApi.dataType.getByName(datePickerType);
      await umbracoUi.dataType.goToDataType(datePickerType);

      // Act
      await umbracoUi.dataType.enterDateFormatValue(dateFormatValue);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      await umbracoUi.dataType.isSuccessNotificationVisible();
      dataTypeData = await umbracoApi.dataType.getByName(datePickerType);
      expect(dataTypeData.values).toEqual(expectedDataTypeValues);
    });
  }
});
