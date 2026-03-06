import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
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
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
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
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesMediaPickerHaveMinAndMaxAmount(customDataTypeName, minValue, maxValue)).toBeTruthy();
});

test('can update enable focal point', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickEnableFocalPointToggle();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'enableLocalFocalPoint', true)).toBeTruthy();
});

test('can add image crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const cropObject = {label: 'Test Label', alias: AliasHelper.toAlias('Test Label'), width: 100, height: 50};
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
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

test('can update ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesToggle();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
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
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
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
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'filter', mediaTypeData.id)).toBeFalsy();
});

test('cannot add a media file as start node', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create media
  const mediaName = 'TestStartNodeFile';
  await umbracoApi.media.createDefaultMediaWithArticle(mediaName);
  expect(await umbracoApi.media.doesNameExist(mediaName)).toBeTruthy();
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickChooseStartNodeButton();

  // Assert
  await umbracoUi.dataType.isMediaCardItemWithNameDisabled(mediaName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can add a media folder as start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaFolderName = 'TestStartNodeFolder';
  const mediaFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickChooseStartNodeButton();
  await umbracoUi.dataType.selectMediaWithName(mediaFolderName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'startNodeId', mediaFolderId)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
});

test('can remove a media folder start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaFolderName = 'TestStartNodeFolder';
  const mediaFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  await umbracoApi.dataType.createMediaPickerDataTypeWithStartNodeId(customDataTypeName, mediaFolderId);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeMediaStartNode(mediaFolderName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'startNodeId', mediaFolderId)).toBeFalsy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
});

test('can add a nested media folder as start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentFolderName = 'ParentMediaFolder';
  const childFolderName = 'ChildMediaFolder';
  await umbracoApi.media.ensureNameNotExists(parentFolderName);
  const parentFolderId = await umbracoApi.media.createDefaultMediaFolder(parentFolderName);
  const childFolderId = await umbracoApi.media.createDefaultMediaFolderAndParentId(childFolderName, parentFolderId);
  await umbracoApi.dataType.createDefaultMediaPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickChooseStartNodeButton();
  await umbracoUi.dataType.clickMediaWithName(parentFolderName);
  await umbracoUi.dataType.selectMediaWithName(childFolderName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'startNodeId', childFolderId)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(parentFolderName);
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
      expect(await umbracoApi.dataType.doesMediaPickerHaveMinAndMaxAmount(mediaPicker.type, 0, 1)).toBeTruthy();
    }
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'startNodeId')).toBeFalsy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'ignoreUserStartNodes')).toBeFalsy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'crops')).toBeFalsy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(mediaPicker.type, 'enableLocalFocalPoint')).toBeFalsy();
  });
}
