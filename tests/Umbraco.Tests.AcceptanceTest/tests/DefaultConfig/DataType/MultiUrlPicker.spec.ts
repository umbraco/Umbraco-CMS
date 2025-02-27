import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multi URL Picker';
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

test('can update minimum number of items value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minimumValue = 2;
  const expectedDataTypeValues = {
    "alias": "minNumber",
    "value": minimumValue
  };

  // Act
  await umbracoUi.dataType.enterMinimumNumberOfItemsValue(minimumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can update maximum number of items value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maximumValue = 2;
  const expectedDataTypeValues = {
    "alias": "maxNumber",
    "value": maximumValue
  };

  // Act
  await umbracoUi.dataType.enterMaximumNumberOfItemsValue(maximumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can enable ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedDataTypeValues = {
    "alias": "ignoreUserStartNodes",
    "value": true
  };

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesSlider();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can update overlay size', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const overlaySizeValue = 'large';
  const expectedDataTypeValues = {
    "alias": "overlaySize",
    "value": overlaySizeValue
  };

  // Act
  await umbracoUi.dataType.chooseOverlaySizeByValue(overlaySizeValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can update hide anchor/query string input', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedDataTypeValues = {
    "alias": "hideAnchor",
    "value": true
  };

  // Act
  await umbracoUi.dataType.clickHideAnchorQueryStringInputSlider();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

// TODO: Remove skip when the front-end is ready. Currently you still can update the minimum greater than the maximum.
test.skip('cannot update the minimum number of items greater than the maximum', async ({umbracoUi}) => {
  // Arrange
  const minimumValue = 5;
  const maximumValue = 2;

  // Act
  await umbracoUi.dataType.enterMinimumNumberOfItemsValue(minimumValue.toString());
  await umbracoUi.dataType.enterMaximumNumberOfItemsValue(maximumValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isErrorNotificationVisible();
});
