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
  const cropObject = {label: 'Test Label', alias: AliasHelper.toAlias('Test Label'), width: 100, height: 50};
  await umbracoApi.dataType.createDefaultImageCropperDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickCreateCropButton();
  await umbracoUi.dataType.enterCropValues(
    cropObject.label,
    cropObject.alias,
    cropObject.width.toString(),
    cropObject.height.toString()
  );
  await umbracoUi.dataType.clickCreateCropButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveCrops(customDataTypeName, cropObject.label, cropObject.alias, cropObject.width, cropObject.height)).toBeTruthy();
});

test('can edit crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const cropObject = {label: 'Test Label', alias: AliasHelper.toAlias('Test Label'), width: 100, height: 50};
  const updatedCropObject = {label: 'Updated Label', alias: AliasHelper.toAlias('Updated Label'), width: 80, height: 30};

  await umbracoApi.dataType.createImageCropperDataTypeWithOneCrop(customDataTypeName, cropObject.label, cropObject.width, cropObject.height);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.editCropByAlias(cropObject.alias);
  await umbracoUi.dataType.enterCropValues(
    updatedCropObject.label,
    updatedCropObject.alias,
    updatedCropObject.width.toString(),
    updatedCropObject.height.toString()
  );
  await umbracoUi.dataType.clickEditCropButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveCrops(customDataTypeName, updatedCropObject.label, updatedCropObject.alias, updatedCropObject.width, updatedCropObject.height)).toBeTruthy();
  expect(await umbracoApi.dataType.doesDataTypeHaveCrops(customDataTypeName, cropObject.label, cropObject.alias, cropObject.width, cropObject.height)).toBeFalsy();
});

test('can delete crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const cropObject = {label: 'Deleted Label', alias: AliasHelper.toAlias('Deleted Label'), width: 50, height: 100};
  await umbracoApi.dataType.createImageCropperDataTypeWithOneCrop(customDataTypeName, cropObject.label, cropObject.width, cropObject.height);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeCropByAlias(cropObject.alias);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveCrops(customDataTypeName, cropObject.label, cropObject.alias, cropObject.width, cropObject.height)).toBeFalsy();
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
