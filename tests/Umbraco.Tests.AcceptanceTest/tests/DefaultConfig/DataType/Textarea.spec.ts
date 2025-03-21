import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Textarea';
let dataTypeDefaultData = null;
let dataTypeData = null;

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoUi.dataType.goToDataType(dataTypeName);
  dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  if (dataTypeDefaultData !== null) {
    await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData);
  }
});

test('can update maximum allowed characters value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxCharsValue = 126;
  const expectedDataTypeValues = {
    "alias": "maxChars",
    "value": maxCharsValue
  };

  // Act
  await umbracoUi.dataType.enterMaximumAllowedCharactersValue(maxCharsValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can update number of rows value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const numberOfRowsValue = 9;
  const expectedDataTypeValues = {
    "alias": "rows",
    "value": numberOfRowsValue
  };

  // Act
  await umbracoUi.dataType.enterNumberOfRowsValue(numberOfRowsValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});
