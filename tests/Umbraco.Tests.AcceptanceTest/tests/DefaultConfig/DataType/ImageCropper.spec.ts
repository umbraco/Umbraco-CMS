import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Image Cropper';
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

test('can add crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const cropData = ['Test Label', 'Test Alias', 100, 50];
  const expectedDataTypeValues = [{
    "alias": "crops",
    "value": [
      {
        "label": cropData[0],
        "alias": cropData[1],
        "width": cropData[2],
        "height": cropData[3]
      }
    ]
  }];
  // Remove all existing crops
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeData.values = [];
  await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
  await umbracoUi.dataType.goToDataType(dataTypeName);

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
  await umbracoUi.dataType.isSuccessNotificationVisible();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual(expectedDataTypeValues);
});

test('can edit crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongCropData = ['Wrong Alias', 50, 100];
  const wrongDataTypeValues = [{
    "alias": "crops",
    "value": [
      {
        "alias": wrongCropData[0],
        "width": wrongCropData[1],
        "height": wrongCropData[2]
      }
    ]
  }];
  const updatedCropData = ['Updated Label', 'Updated Test Alias', 100, 50];
  const expectedDataTypeValues = [{
    "alias": "crops",
    "value": [
      {
        "label": updatedCropData[0],
        "alias": updatedCropData[1],
        "width": updatedCropData[2],
        "height": updatedCropData[3]
      }
    ]
  }];
  // Remove all existing crops and add a crop to edit
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeData.values = wrongDataTypeValues;
  await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.editCropByAlias(wrongCropData[0].toString());
  await umbracoUi.dataType.enterCropValues(updatedCropData[0].toString(), updatedCropData[1].toString(), updatedCropData[2].toString(), updatedCropData[3].toString());
  await umbracoUi.dataType.clickSaveCropButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual(expectedDataTypeValues);
});

test('can delete crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongCropData = ['Wrong Alias', 50, 100];
  const wrongDataTypeValues = [{
    "alias": "crops",
    "value": [
      {
        "alias": wrongCropData[0],
        "width": wrongCropData[1],
        "height": wrongCropData[2]
      }
    ]
  }];
  // Remove all existing crops and add a crop to remove
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeData.values = wrongDataTypeValues;
  await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.removeCropByAlias(wrongCropData[0].toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual([]);
});
