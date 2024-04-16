import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multiple Media Picker';
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

  test('can update pick multiple items', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      "alias": "multiple",
      "value": false,
    };

    // Act
    await umbracoUi.dataType.clickPickMultipleItemsSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can update amount', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const lowValue = 5;
    const highValue = 1000;
    const expectedDataTypeValues = {
      "alias": "validationLimit",
      "value": {
        "min": lowValue,
        "max": highValue
      }
    };

    // Act
    await umbracoUi.dataType.enterAmountValue(lowValue.toString(), highValue.toString());
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can update enable focal point', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      "alias": "enableLocalFocalPoint",
      "value": true,
    };

    // Act
    await umbracoUi.dataType.clickEnableFocalPointSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can add image crop', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const cropData = ['Test Label', 'Test Alias', 100, 50];
    const expectedDataTypeValues = {
      "alias": "crops",
      "value": [
        {
          "label": cropData[0],
          "alias": cropData[1],
          "width": cropData[2],
          "height": cropData[3]
        }
      ]
    };

    // Act
    await umbracoUi.dataType.enterCropValues(
      cropData[0].toString(),
      cropData[1].toString(),
      cropData[2].toString(),
      cropData[3].toString()
    );
    await umbracoUi.dataType.clickAddCropButton();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  test('can update ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedDataTypeValues = {
      "alias": "ignoreUserStartNodes",
      "value": true,
    };

    // Act
    await umbracoUi.dataType.clickIgnoreUserStartNodesCamelSlider();
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
  });

  // TODO: implement this test when the frontend is ready
  test.skip('can add accepted types', async ({umbracoApi, umbracoUi}) => {

  });

  // TODO: implement this test when the frontend is ready
  test.skip('can add start node', async ({umbracoApi, umbracoUi}) => {

  });
});
