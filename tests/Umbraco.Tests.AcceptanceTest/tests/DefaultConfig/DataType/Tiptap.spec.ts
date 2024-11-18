import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const tiptapName = 'TestTiptap';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(tiptapName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(tiptapName);
});

test('can create a rich text editor tiptap', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  //  Arrange
  const tiptapLocatorName = 'Rich Text Editor [Tiptap]';
  const tiptapAlias = 'Umbraco.RichText';
  const tiptapUiAlias = 'Umb.PropertyEditorUi.Tiptap';

  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickCreateButton();
  await umbracoUi.dataType.clickNewDataTypeThreeDotsButton();
  await umbracoUi.dataType.enterDataTypeName(tiptapName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(tiptapLocatorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesNameExist(tiptapName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(tiptapName);
  expect(dataTypeData.editorAlias).toBe(tiptapAlias);
  expect(dataTypeData.editorUiAlias).toBe(tiptapUiAlias);
});

test('can rename a rich text editor tiptap', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'TiptapTest';
  await umbracoApi.dataType.createDefaultTiptapDataType(wrongName);

  // Act
  await umbracoUi.dataType.goToDataType(wrongName);
  await umbracoUi.dataType.enterDataTypeName(tiptapName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesNameExist(tiptapName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(wrongName)).toBeFalsy();
});

test('can delete a rich text editor tiptap', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultTiptapDataType(tiptapName);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(tiptapName);
  await umbracoUi.dataType.clickDeleteAndConfirmButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesNameExist(tiptapName)).toBeFalsy();
  await umbracoUi.dataType.isTreeItemVisible(tiptapName, false);
});

test('can add dimensions', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const width = 100;
  const height = 10;
  const expectedTiptapValues = {
    "alias": "dimensions",
    "value": {
      "width": width,
      "height": height
    }
  };
  await umbracoApi.dataType.createDefaultTiptapDataType(tiptapName);
  await umbracoUi.dataType.goToDataType(tiptapName);

  // Act
  await umbracoUi.dataType.enterDimensionsValue(width.toString(), height.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const tiptapData = await umbracoApi.dataType.getByName(tiptapName);
  expect(tiptapData.values).toContainEqual(expectedTiptapValues);
});

test('can update maximum size for inserted images', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maximumSize = 300;
  const expectedTiptapValues = {
    "alias": "maxImageSize",
    "value": maximumSize
  };
  await umbracoApi.dataType.createDefaultTiptapDataType(tiptapName);
  await umbracoUi.dataType.goToDataType(tiptapName);

  // Act
  await umbracoUi.dataType.enterMaximumSizeForImages(maximumSize.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const tiptapData = await umbracoApi.dataType.getByName(tiptapName);
  expect(tiptapData.values).toContainEqual(expectedTiptapValues);
});

test('can select overlay size', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const overlaySizeValue = 'large';
  const expectedTiptapValues = {
    "alias": "overlaySize",
    "value": overlaySizeValue
  };
  await umbracoApi.dataType.createDefaultTiptapDataType(tiptapName);
  await umbracoUi.dataType.goToDataType(tiptapName);

  // Act
  await umbracoUi.dataType.chooseOverlaySizeByValue(overlaySizeValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const tiptapData = await umbracoApi.dataType.getByName(tiptapName);
  expect(tiptapData.values).toContainEqual(expectedTiptapValues);
});

test('can add an available block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeName = 'TestElementType';
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const expectedTiptapValues = {
    alias: "blocks",
    value: [
      {
        contentElementTypeKey: elementTypeId,
      }
    ]
  };
  await umbracoApi.dataType.createDefaultTiptapDataType(tiptapName);
  await umbracoUi.dataType.goToDataType(tiptapName);

  // Act
  await umbracoUi.dataType.addAvailableBlocks(elementTypeName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const tiptapData = await umbracoApi.dataType.getByName(tiptapName);
  expect(tiptapData.values).toContainEqual(expectedTiptapValues);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can add image upload folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaFolderName = 'TestMediaFolder';
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  const mediaFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  const expectedTiptapValues = {
    "alias": "mediaParentId",
    "value": mediaFolderId
  };
  await umbracoApi.dataType.createDefaultTiptapDataType(tiptapName);
  await umbracoUi.dataType.goToDataType(tiptapName);

  // Act
  await umbracoUi.dataType.addImageUploadFolder(mediaFolderName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const tiptapData = await umbracoApi.dataType.getByName(tiptapName);
  expect(tiptapData.values).toContainEqual(expectedTiptapValues);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
});

test('can enable ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedTiptapValues = {
    "alias": "ignoreUserStartNodes",
    "value": true
  };
  await umbracoApi.dataType.createDefaultTiptapDataType(tiptapName);
  await umbracoUi.dataType.goToDataType(tiptapName);

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesSlider();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const tiptapData = await umbracoApi.dataType.getByName(tiptapName);
  expect(tiptapData.values).toContainEqual(expectedTiptapValues);
});

test('can delete toolbar group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const deletedGroupIndex = 2;
  const rowIndex = 0;
  await umbracoApi.dataType.createDefaultTiptapDataType(tiptapName);
  await umbracoUi.dataType.goToDataType(tiptapName);
  const groupCount = await umbracoApi.dataType.getTiptapToolbarGroupInRowCount(tiptapName, rowIndex);
  const groupValue = await umbracoApi.dataType.getTiptapToolbarGroupValueInRow(tiptapName, deletedGroupIndex, rowIndex);

  // Act
  await umbracoUi.dataType.deleteToolbarGroup(deletedGroupIndex, rowIndex);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const tipTapData = await umbracoApi.dataType.getByName(tiptapName);
  const toolbarValue = tipTapData.values.find(value => value.alias === 'toolbar');
  expect(toolbarValue.value[rowIndex].length).toBe(groupCount - 1);
  expect(toolbarValue.value[rowIndex]).not.toContain(groupValue);
});

test('can delete toolbar row', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const deletedRowIndex = 1;
  await umbracoApi.dataType.createTiptapDataTypeWithTwoToolbarRows(tiptapName);
  await umbracoUi.dataType.goToDataType(tiptapName);
  const rowCount = await umbracoApi.dataType.getTiptapToolbarRowCount(tiptapName);

  // Act
  await umbracoUi.dataType.deleteToolbarRow(deletedRowIndex);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const tipTapData = await umbracoApi.dataType.getByName(tiptapName);
  const toolbarValue = tipTapData.values.find(value => value.alias === 'toolbar');
  if (rowCount - 1 === 0) {
    expect(toolbarValue).toBeFalsy();
  } else {
    expect(toolbarValue.value.length).toBe(rowCount - 1);
  }
});

test('can disable extensions item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const extensionItemName = 'Text Align';
  await umbracoApi.dataType.createDefaultTiptapDataType(tiptapName);
  await umbracoUi.dataType.goToDataType(tiptapName);
  const extensionsCount = await umbracoApi.dataType.getTiptapExtensionsCount(tiptapName);

  // Act
  await umbracoUi.dataType.clickExtensionItemHaveName(extensionItemName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  const tipTapData = await umbracoApi.dataType.getByName(tiptapName);
  const extensionsValue = tipTapData.values.find(value => value.alias === 'extensions');
  expect(extensionsValue.value.length).toBe(extensionsCount - 1);
  expect(extensionsValue.value).not.toContain(extensionItemName);
});