import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Image Cropper';
const editorAlias = 'Umbraco.ImageCropper';
const editorUiAlias = 'Umb.PropertyEditorUi.ImageCropper';
const customDataTypeName = 'Custom Image Cropper';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can add crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const cropData = ['Test Label', 'Test Alias', 100, 50];
  await umbracoApi.dataType.createDefaultImageCropperDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

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
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveCrops(customDataTypeName, cropData[0], cropData[1], cropData[2], cropData[3])).toBeTruthy();
});

test('can edit crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const cropData = ['Test Label', AliasHelper.toAlias('Test Label'), 100, 50];
  const updatedCropData = ['Updated Label', AliasHelper.toAlias('Updated Label'), 80, 30];
  await umbracoApi.dataType.createImageCropperDataTypeWithOneCrop(customDataTypeName, cropData[0], cropData[2], cropData[3]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.editCropByAlias(cropData[0]);
  await umbracoUi.dataType.enterCropValues(updatedCropData[0], updatedCropData[1], updatedCropData[2].toString(), updatedCropData[3].toString());
  await umbracoUi.dataType.clickSaveCropButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveCrops(customDataTypeName, updatedCropData[0], updatedCropData[1], updatedCropData[2], updatedCropData[3])).toBeTruthy();
  expect(await umbracoApi.dataType.doesDataTypeHaveCrops(customDataTypeName, cropData[0], cropData[1], cropData[2], cropData[3])).toBeFalsy();
});

test('can delete crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const cropData = ['Deleted Alias', AliasHelper.toAlias('Deleted Alias'), 50, 100];
  await umbracoApi.dataType.createImageCropperDataTypeWithOneCrop(customDataTypeName, cropData[0], cropData[2], cropData[3]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeCropByAlias(cropData[0].toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveCrops(customDataTypeName, cropData[0], cropData[1], cropData[2], cropData[3])).toBeFalsy();
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.imageCropperSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.imageCropperSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'crops')).toBeFalsy();
});