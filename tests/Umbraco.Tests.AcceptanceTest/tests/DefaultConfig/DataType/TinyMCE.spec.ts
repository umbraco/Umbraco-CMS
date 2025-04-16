import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const tinyMCEName = 'TestTinyMCE';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(tinyMCEName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(tinyMCEName);
});

test('can create a rich text editor with tinyMCE', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  //  Arrange
  const tinyMCELocatorName = 'Rich Text Editor [TinyMCE]';
  const tinyMCEFilterKeyword = 'Rich Text Editor';
  const tinyMCEAlias = 'Umbraco.RichText';
  const tinyMCEUiAlias = 'Umb.PropertyEditorUi.TinyMCE';
  const toolbarValue = [
    "styles",
    "bold",
    "italic",
    "alignleft",
    "aligncenter",
    "alignright",
    "bullist",
    "numlist",
    "outdent",
    "indent",
    "sourcecode",
    "link",
    "umbmediapicker",
    "umbembeddialog"
  ];

  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickActionsMenuCreateButton();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(tinyMCEName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(tinyMCELocatorName, tinyMCEFilterKeyword);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.dataType.doesNameExist(tinyMCEName)).toBeTruthy();
  // Verify the default configuration
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.tinyMCESettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.tinyMCESettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(tinyMCEAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(tinyMCEUiAlias);
  const dataTypeData = await umbracoApi.dataType.getByName(tinyMCEName);
  expect(dataTypeData.editorAlias).toBe(tinyMCEAlias);
  expect(dataTypeData.editorUiAlias).toBe(tinyMCEUiAlias);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tinyMCEName, 'maxImageSize', 500)).toBeTruthy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tinyMCEName, 'mode', 'Classic')).toBeTruthy();
  expect(await umbracoApi.dataType.doesTinyMCEToolbarHaveItems(tinyMCEName, toolbarValue)).toBeTruthy();
});

test('can rename a rich text editor with tinyMCE', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'tinyMCETest';
  await umbracoApi.dataType.createDefaultTinyMCEDataType(wrongName);

  // Act
  await umbracoUi.dataType.goToDataType(wrongName);
  await umbracoUi.dataType.enterDataTypeName(tinyMCEName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesNameExist(tinyMCEName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(wrongName)).toBeFalsy();
});

test('can delete a rich text editor with tinyMCE', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(tinyMCEName);
  await umbracoUi.dataType.clickDeleteAndConfirmButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  expect(await umbracoApi.dataType.doesNameExist(tinyMCEName)).toBeFalsy();
  await umbracoUi.dataType.isTreeItemVisible(tinyMCEName, false);
});

test('can enable toolbar options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const toolbarValues = ["undo", "redo", "cut"];
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);
  const toolbarItemCount = await umbracoApi.dataType.getTinyMCEToolbarItemsCount(tinyMCEName);

  // Act
  await umbracoUi.dataType.clickToolbarOptionByValue(toolbarValues);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const expectedToolbarItems = toolbarItemCount + toolbarValues.length;
  expect(await umbracoApi.dataType.doesTinyMCEToolbarItemsMatchCount(tinyMCEName, expectedToolbarItems)).toBeTruthy();
  expect(await umbracoApi.dataType.doesTinyMCEToolbarHaveItems(tinyMCEName, toolbarValues)).toBeTruthy();
});

test('can add stylesheet', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetName = 'StylesheetForDataType.css';
  const stylesheetPath = await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.addStylesheet(stylesheetName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tinyMCEName, 'stylesheets', [stylesheetPath])).toBeTruthy();

  // Clean
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
});

test('can add dimensions', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const width = 100;
  const height = 10;
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.enterDimensionsValue(width.toString(), height.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesRTEHaveDimensions(tinyMCEName, width, height)).toBeTruthy();
});

test('can update maximum size for inserted images', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maximumSize = 300;
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.enterMaximumSizeForImages(maximumSize.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tinyMCEName, 'maxImageSize', maximumSize)).toBeTruthy();
});

test('can enable inline editing mode', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mode = 'Inline';
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.clickInlineRadioButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tinyMCEName, 'mode', mode)).toBeTruthy();
});

test('can add an available block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeName = 'TestElementType';
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);

  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.addAvailableBlocks(elementTypeName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesRTEContainBlocks(tinyMCEName, [elementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can select overlay size', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const overlaySizeValue = 'large';
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.chooseOverlaySizeByValue(overlaySizeValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tinyMCEName, 'overlaySize', overlaySizeValue)).toBeTruthy();
});

test('can enable hide label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.clickHideLabelToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tinyMCEName, 'hideLabel', true)).toBeTruthy();
});

test('can add image upload folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaFolderName = 'TestMediaFolder';
  const mediaFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.addImageUploadFolder(mediaFolderName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tinyMCEName, 'mediaParentId', mediaFolderId)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
});

test('can enable ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultTinyMCEDataType(tinyMCEName);
  await umbracoUi.dataType.goToDataType(tinyMCEName);

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(tinyMCEName, 'ignoreUserStartNodes', true)).toBeTruthy();
});