import {NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const tipTapName = 'TestTiptap';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(tipTapName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(tipTapName);
});

test('can create a rich text editor with tiptap', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  //  Arrange
  const tipTapLocatorName = 'Rich Text Editor [Tiptap]';
  const tipTapAlias = 'Umbraco.RichText';
  const tipTapUiAlias = 'Umb.PropertyEditorUi.Tiptap';

  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickActionsMenuCreateButton();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(tipTapName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(tipTapLocatorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(tipTapName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(tipTapName);
  expect(dataTypeData.editorAlias).toBe(tipTapAlias);
  expect(dataTypeData.editorUiAlias).toBe(tipTapUiAlias);
});

test('can rename a rich text editor with tiptap', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'TiptapTest';
  await umbracoApi.dataType.createDefaultTiptapDataType(wrongName);

  // Act
  await umbracoUi.dataType.goToDataType(wrongName);
  await umbracoUi.dataType.enterDataTypeName(tipTapName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(tipTapName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(wrongName)).toBeFalsy();
});

test('can delete a rich text editor with tiptap', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(tipTapName);
  await umbracoUi.dataType.clickDeleteAndConfirmButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(tipTapName)).toBeFalsy();
  await umbracoUi.dataType.isTreeItemVisible(tipTapName, false);
});

test('can add dimensions', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const width = 100;
  const height = 10;
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);

  // Act
  await umbracoUi.dataType.enterDimensionsValue(width.toString(), height.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
expect(await umbracoApi.dataType.doesRTEHaveDimensions(tipTapName, width, height)).toBeTruthy();
});

test('can update maximum size for inserted images', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maximumSize = 300;
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);

  // Act
  await umbracoUi.dataType.enterMaximumSizeForImages(maximumSize.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tipTapName, 'maxImageSize', maximumSize)).toBeTruthy();
});

test('can select overlay size', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const overlaySizeValue = 'large';
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);

  // Act
  await umbracoUi.dataType.chooseOverlaySizeByValue(overlaySizeValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tipTapName, 'overlaySize', overlaySizeValue)).toBeTruthy();
});

test('can add an available block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeName = 'TestElementType';
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);

  // Act
  await umbracoUi.dataType.isExtensionItemChecked('Block', false);
  await umbracoUi.dataType.addAvailableBlocks(elementTypeName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesRTEContainBlocks(tipTapName, [elementTypeId])).toBeTruthy();
  // Verify that "Block" extension is enable
  await umbracoUi.dataType.isExtensionItemChecked('Block');

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can add image upload folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaFolderName = 'TestMediaFolder';
  const mediaFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);

  // Act
  await umbracoUi.dataType.addImageUploadFolder(mediaFolderName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tipTapName, 'mediaParentId', mediaFolderId)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
});

test('can enable ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tipTapName, 'ignoreUserStartNodes', true)).toBeTruthy();
});

test('can delete toolbar group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const deletedGroupIndex = 2;
  const rowIndex = 0;
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);
  const groupCount = await umbracoApi.dataType.getTiptapToolbarGroupInRowCount(tipTapName, rowIndex);
  const groupValue = await umbracoApi.dataType.getTiptapToolbarGroupValueInRow(tipTapName, deletedGroupIndex, rowIndex);

  // Act
  await umbracoUi.dataType.deleteToolbarGroup(deletedGroupIndex, rowIndex);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  const tipTapData = await umbracoApi.dataType.getByName(tipTapName);
  const toolbarValue = tipTapData.values.find(value => value.alias === 'toolbar');
  expect(toolbarValue.value[rowIndex].length).toBe(groupCount - 1);
  expect(toolbarValue.value[rowIndex]).not.toContain(groupValue);
});

test('can delete toolbar row', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const deletedRowIndex = 1;
  await umbracoApi.dataType.createTiptapDataTypeWithTwoToolbarRows(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);
  const rowCount = await umbracoApi.dataType.getTiptapToolbarRowCount(tipTapName);

  // Act
  await umbracoUi.dataType.deleteToolbarRow(deletedRowIndex);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  const tipTapData = await umbracoApi.dataType.getByName(tipTapName);
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
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);
  const extensionsCount = await umbracoApi.dataType.getTiptapExtensionsCount(tipTapName);

  // Act
  await umbracoUi.dataType.clickExtensionItemWithName(extensionItemName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  const tipTapData = await umbracoApi.dataType.getByName(tipTapName);
  const extensionsValue = tipTapData.values.find(value => value.alias === 'extensions');
  expect(extensionsValue.value.length).toBe(extensionsCount - 1);
  expect(extensionsValue.value).not.toContain(extensionItemName);
});

test('can add a statusbar', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const statusbarName = 'Word Count';
  const statusbarApiValue = 'Umb.Tiptap.Statusbar.WordCount';
  await umbracoApi.dataType.createDefaultTiptapDataType(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);

  // Act
  await umbracoUi.dataType.clickStatusbarItemInToolboxWithName(statusbarName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const tipTapData = await umbracoApi.dataType.getByName(tipTapName);
  const statusbarValue = tipTapData.values.find(value => value.alias === 'statusbar');
  expect(statusbarValue.value).toEqual([[statusbarApiValue]]);
});

test('can remove a statusbar', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const statusbarName = 'Word Count';
  await umbracoApi.dataType.createTiptapDataTypeWithWordCountStatusbar(tipTapName);
  await umbracoUi.dataType.goToDataType(tipTapName);

  // Act
  await umbracoUi.dataType.clickStatusbarItemWithName(statusbarName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const tipTapData = await umbracoApi.dataType.getByName(tipTapName);
  const statusbarValue = tipTapData.values.find(value => value.alias === 'statusbar');
  expect(statusbarValue).toBeFalsy();
});