import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Approved Color';
let dataTypeDefaultData = null;
let dataTypeData = null;
const colorValue = 'ffffff';
const colorLabel = '';

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
  await umbracoUi.dataType.clickIncludeLabelsToggle();
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
  // Remove all existing values
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeData.values = [];
  await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.addColor(colorValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual(expectedDataTypeValues);
});

test('can remove color', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.dataType.isSuccessNotificationVisible();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual([]);
});
