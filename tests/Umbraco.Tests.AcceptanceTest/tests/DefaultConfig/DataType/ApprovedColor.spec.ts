import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Approved Color';
test.describe(`${dataTypeName} tests`, () => {
  let dataTypeDefaultData = null;
  let dataTypeData = null;
  const colorValue = '#ffffff';
  const colorLabel = 'TestColor';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
    dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
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
    // Remove all existing values
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    dataTypeData.values = [];
    await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
    await umbracoUi.dataType.goToDataType(dataTypeName);

    // Act
    await umbracoUi.dataType.clickIncludeLabelsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toEqual(expectedDataTypeValues);
  });

  //TODO: Remove skip when the frontend is ready
  test.skip('can add color', async ({umbracoApi, umbracoUi}) => {
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
    // Remove all existing values
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    dataTypeData.values = [];
    await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
    await umbracoUi.dataType.goToDataType(dataTypeName);

    // Act
    await umbracoUi.dataType.addColor(colorValue, colorLabel);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toEqual(expectedDataTypeValues);
  });

  // TODO: remove .skip when the frontend is able to display the added color. Currently the added colors are not displayed after reloading page
  test.skip('can remove color', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const removedDataTypeValues = [
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
    // Remove all existing values and add a color to remove
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    dataTypeData.values = removedDataTypeValues;
    await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
    await umbracoUi.dataType.goToDataType(dataTypeName);

    // Act
    await umbracoUi.dataType.removeColorByValue(colorValue);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toEqual([]);
  });
});
