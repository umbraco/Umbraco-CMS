import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Textarea';
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
    const maxCharsValue = 126;
    const expectedDataTypeValues = {
      alias: "maxChars",
      value: maxCharsValue,
    };

    // Act
    await umbracoUi.dataType.enterMaximumAllowedCharactersValue(maxCharsValue.toString());
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can update Number of rows value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const numberOfRowsValue = 9;
    const expectedDataTypeValues = {
      alias: "rows",
      value: numberOfRowsValue,
    };

    // Act
    await umbracoUi.dataType.enterNumberOfRowsValue(numberOfRowsValue.toString());
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can update Min height (pixels) value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const minHeightValue = 150;
    const expectedDataTypeValues = {
      alias: "minHeight",
      value: minHeightValue,
    };

    // Act
    await umbracoUi.dataType.enterMinHeightValue(minHeightValue.toString());
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can update Max height (pixels) value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const maxHeightValue = 300;
    const expectedDataTypeValues = {
      alias: "maxHeight",
      value: maxHeightValue,
    };

    // Act
    await umbracoUi.dataType.enterMaxHeightValue(maxHeightValue.toString());
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });
});
