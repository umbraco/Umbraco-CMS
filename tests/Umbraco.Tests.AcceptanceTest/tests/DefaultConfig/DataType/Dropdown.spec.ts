import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Dropdown';
let dataTypeDefaultData = null;
let dataTypeData = null;

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

test('can enable multiple choice', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedDataTypeValues = [{
    "alias": "multiple",
    "value": true
  }];
  // Remove all existing options
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeData.values = [];
  await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickEnableMultipleChoiceToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual(expectedDataTypeValues);
});

test('can add option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const optionName = 'Test option';
  const expectedDataTypeValues = [
    {
      "alias": "items",
      "value": [optionName]
    }
  ];
  // Remove all existing options
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeData.values = [];
  await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickAddOptionButton();
  await umbracoUi.dataType.enterOptionName(optionName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual(expectedDataTypeValues);
});

test('can remove option', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const removedOptionName = 'Removed Option';
  const removedOptionValues = [
    {
      "alias": "items",
      "value": [removedOptionName]
    }
  ];
  // Remove all existing options and add an option to remove
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeData.values = removedOptionValues;
  await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.removeOptionByName(removedOptionName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual([]);
});
