import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Approved Color tests', () => {
  let dataTypeDefaultData = null;
  let dataTypeData = null;
  const dataTypeName = 'Approved Color';
  const colorValue = '#ffffff';
  const colorLabel = 'TestColor';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
    await umbracoUi.dataType.goToDataType(dataTypeName);
    dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
    dataTypeDefaultData.values = [];
    await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData);  
  });

  test.afterEach(async ({umbracoApi}) => {
    if (dataTypeDefaultData !== null) {
      await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData);   
    }   
  });

  test('can include label', async ({umbracoApi, umbracoUi}) => {  
    // Arrange
    const expectedDataTypeValues = [
      {
        "alias": "useLabel",
        "value": true
      }
    ];

    // Act
    await umbracoUi.dataType.clickIncludeLabelsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toEqual(expectedDataTypeValues);
  });

  test('can add color', async ({umbracoApi, umbracoUi}) => {  
    // Arrange
    const expectedDataTypeValues = [
      {
        "alias": "items",
        "value": [
          {
            "value": colorValue,
            "label": colorLabel
          }
        ]
      }
    ];

    // Act
    await umbracoUi.dataType.addColor(colorValue, colorLabel);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toEqual(expectedDataTypeValues);
  });

  test('can remove color', async ({umbracoApi, umbracoUi}) => {  
    // Arrange
    const dataTypeValues = [
      {
        "alias": "items",
        "value": [
          {
            "value": colorValue,
            "label": colorLabel
          }
        ]
      }
    ];
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    dataTypeData.values = dataTypeValues;
    await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);  
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.dataType.removeColorByValue(colorValue);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toEqual([]);
  });
});
