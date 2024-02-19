import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Numeric';
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

  test('can update Minimum value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const minimumValue = -5;
    const expectedDataTypeValues = {
      alias: "min",
      value: minimumValue,
    };

    // Act
    await umbracoUi.dataType.enterMinimumValue(minimumValue.toString());
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can update Maximum value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const maximumValue = 1000000;
    const expectedDataTypeValues = {
      alias: "max",
      value: maximumValue,
    };

    // Act
    await umbracoUi.dataType.enterMaximumValue(maximumValue.toString());
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can update Step Size value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const stepSizeValue = 5;
    const expectedDataTypeValues = {
      alias: "step",
      value: stepSizeValue,
    };

    // Act
    await umbracoUi.dataType.enterStepSizeValue(stepSizeValue.toString());
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });
});
