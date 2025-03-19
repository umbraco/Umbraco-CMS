import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const mediaPickerTypes = [
  {type: 'Media Picker', isMultiple: false},
  {type: 'Multiple Media Picker', isMultiple: true},
  {type: 'Image Media Picker', isMultiple: false},
  {type: 'Multiple Image Media Picker', isMultiple: true},
];
const editorAlias = 'Umbraco.MediaPicker3';
const editorUiAlias = 'Umb.PropertyEditorUi.MediaPicker';
const customDataTypeName = 'Custom Media Picker';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can update pick multiple items', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickPickMultipleItemsToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'multiple', true)).toBeTruthy();
});

test('can update amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minValue = 5;
  const maxValue = 1000;
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterAmountValue(minValue.toString(), maxValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesMediaPickerHaveAmount(customDataTypeName, minValue, maxValue)).toBeTruthy();
});

test('can update enable focal point', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickEnableFocalPointToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'enableLocalFocalPoint', true)).toBeTruthy();
});

test('can add image crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const cropData = ['Test Label', 'testAlias', 100, 50];
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
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

test('can update ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'ignoreUserStartNodes', true)).toBeTruthy();
});

test('can add accepted types', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaTypeName = 'Audio';
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.addAcceptedType(mediaTypeName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'filter', mediaTypeData.id)).toBeTruthy();
});

test('can remove accepted types', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaTypeName = 'Image';
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  await umbracoApi.dataType.createImageMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeAcceptedType(mediaTypeName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'filter', mediaTypeData.id)).toBeFalsy();
});

test('can add start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create media
  const mediaName = 'TestStartNode';
  const mediaId = await umbracoApi.media.createDefaultMediaWithArticle(mediaName);
  expect(await umbracoApi.media.doesNameExist(mediaName)).toBeTruthy();
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickChooseStartNodeButton();
  await umbracoUi.dataType.addMediaStartNode(mediaName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'startNodeId', mediaId)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can remove start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create media
  const mediaName = 'TestStartNode';
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const mediaId = await umbracoApi.media.createDefaultMediaWithArticle(mediaName);
  expect(await umbracoApi.media.doesNameExist(mediaName)).toBeTruthy();
  await umbracoApi.dataType.createImageMediaPickerDataTypeWithStartNodeId(customDataTypeName, mediaId);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeMediaStartNode(mediaName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'startNodeId', mediaId)).toBeFalsy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

for (const mediaPicker of mediaPickerTypes) {
  test(`the default configuration of ${mediaPicker.type} is correct`, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.goToDataType(mediaPicker.type);

    // Assert
    await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.mediaPickerSettings);
    await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.mediaPickerSettings);
    await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
    await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'multiple', mediaPicker.isMultiple)).toBeTruthy();
    if (mediaPicker.type.includes('Image')) {
      const imageTypeData = await umbracoApi.mediaType.getByName('Image');
      expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'filter', imageTypeData.id)).toBeTruthy();
    }
    if (!mediaPicker.type.includes('Multiple')) {
      expect(await umbracoApi.dataType.doesMediaPickerHaveAmount(mediaPicker.type, 0, 1)).toBeTruthy();
    }
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'startNodeId')).toBeFalsy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'ignoreUserStartNodes')).toBeFalsy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'crops')).toBeFalsy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'enableLocalFocalPoint')).toBeFalsy();
  });
}
