﻿import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Textstring';
test.describe(`${dataTypeName} tests`, () => {
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

  test('can update Maximum allowed characters value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const maxCharsValue = '126';
    const expectedDataTypeValues = {
      alias: "maxChars",
      value: maxCharsValue,
    };

    // Act
    await umbracoUi.dataType.enterMaximumAllowedCharactersValue(maxCharsValue);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });
});
