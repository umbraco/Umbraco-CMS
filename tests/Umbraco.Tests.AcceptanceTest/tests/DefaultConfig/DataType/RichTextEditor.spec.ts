import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Rich Text Editor';
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

  test('can select ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      alias: "multiple",
      value: false,
    };

    // Act
    await umbracoUi.dataType.clickPickMultipleItemsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can enable all toolbar options', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      alias: "multiple",
      value: false,
    };

    // Act
    await umbracoUi.dataType.clickPickMultipleItemsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can add stylesheet', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      alias: "multiple",
      value: false,
    };

    // Act
    await umbracoUi.dataType.clickPickMultipleItemsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can add dimensions', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      alias: "multiple",
      value: false,
    };

    // Act
    await umbracoUi.dataType.clickPickMultipleItemsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can add maximum size for inserted images', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      alias: "multiple",
      value: false,
    };

    // Act
    await umbracoUi.dataType.clickPickMultipleItemsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can select overlay size', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      alias: "multiple",
      value: false,
    };

    // Act
    await umbracoUi.dataType.clickPickMultipleItemsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can enable hide label', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      alias: "multiple",
      value: false,
    };

    // Act
    await umbracoUi.dataType.clickPickMultipleItemsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  // TODO: implement this test when the frontend is ready
  test.skip('can add Image upload folder', async ({umbracoApi, umbracoUi}) => {

  });

  // TODO: implement this test when the frontend is ready
  test.skip('can select mode', async ({umbracoApi, umbracoUi}) => {

  });

  // TODO: implement this test when the frontend is ready
  test.skip('can add available blocks', async ({umbracoApi, umbracoUi}) => {

  });
});
