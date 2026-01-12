import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const blockGridEditorName = 'TestBlockGridEditor';
const elementTypeName = 'BlockGridElement';
const dataTypeName = 'Textstring';
const groupName = 'testGroup';
let contentElementTypeId = '';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridEditorName);
});

// TODO: Remove skip and update test when the front-end is ready. Currently it is not possible to add a custom view to a block
test.skip('can add a custom view to a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  // TODO: Implement it later
});

// TODO: Remove skip and update test when the front-end is ready. Currently it is not possible to add a custom view to a block
test.skip('can remove a custom view from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  // TODO: Implement it later
});

// TODO: Remove skip and update test when the front-end is ready. Currently it is not possible to add a custom stylesheet to a block
test.skip('can remove a custom stylesheet from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetName = 'TestStylesheet.css'
  const stylesheetPath = '/wwwroot/css/' + stylesheetName;
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);
  await umbracoApi.dataType.createBlockGridWithAdvancedSettingsInBlock(blockGridEditorName, contentElementTypeId, undefined, stylesheetPath, undefined, undefined, undefined);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainStylesheet(blockGridEditorName, contentElementTypeId, stylesheetPath)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  // TODO: Implement it later
});

test('can update overlay size in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const overlaySize = 'medium';
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.updateBlockOverlaySize(overlaySize);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainOverlaySize(blockGridEditorName, contentElementTypeId, overlaySize)).toBeTruthy();
});

test('can enable inline editing mode in a block', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.clickInlineEditingMode();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainInlineEditing(blockGridEditorName, contentElementTypeId, true)).toBeTruthy();
});

test('can disable inline editing mode in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockGridWithAdvancedSettingsInBlock(blockGridEditorName, contentElementTypeId, undefined, undefined, 'small', true);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainInlineEditing(blockGridEditorName, contentElementTypeId, true)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.clickInlineEditingMode();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainInlineEditing(blockGridEditorName, contentElementTypeId, false)).toBeTruthy();
});

test('can enable hide content editor in a block', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.clickBlockGridHideContentEditorButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainHideContentEditor(blockGridEditorName, contentElementTypeId, true)).toBeTruthy();
});

test('can disable hide content editor in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockGridWithAdvancedSettingsInBlock(blockGridEditorName, contentElementTypeId, undefined, undefined, 'small', false, true);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainHideContentEditor(blockGridEditorName, contentElementTypeId, true)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.clickBlockGridHideContentEditorButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainHideContentEditor(blockGridEditorName, contentElementTypeId, false)).toBeTruthy();
});

test('can add a background color to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backGroundColor = '#000000';
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.selectBlockBackgroundColor(backGroundColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainBackgroundColor(blockGridEditorName, contentElementTypeId, backGroundColor)).toBeTruthy();
});

test('can remove a background color to a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backGroundColor = '#000000';
  await umbracoApi.dataType.createBlockGridWithCatalogueAppearanceInBlock(blockGridEditorName, contentElementTypeId, backGroundColor);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainBackgroundColor(blockGridEditorName, contentElementTypeId, backGroundColor)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.selectBlockBackgroundColor('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainBackgroundColor(blockGridEditorName, contentElementTypeId, '')).toBeTruthy();
});

test('can add a icon color to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const iconColor = '#000000';
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.selectBlockIconColor(iconColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainIconColor(blockGridEditorName, contentElementTypeId, iconColor)).toBeTruthy();
});

test('can remove a icon color from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const iconColor = '#000000';
  await umbracoApi.dataType.createBlockGridWithCatalogueAppearanceInBlock(blockGridEditorName, contentElementTypeId, '', iconColor);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainIconColor(blockGridEditorName, contentElementTypeId, iconColor)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.selectBlockIconColor('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainIconColor(blockGridEditorName, contentElementTypeId, '')).toBeTruthy();
});

test('can add a thumbnail to a block', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaName = 'TestMedia';
  const mediaId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);
  const mediaUrl = await umbracoApi.media.getFullMediaUrl(mediaId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.chooseBlockThumbnailWithPath(mediaUrl);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  await umbracoUi.dataType.doesBlockHaveThumbnailImage(elementTypeName, mediaUrl);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can remove a thumbnail from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaName = 'TestMedia';
  await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const mediaData = await umbracoApi.media.getByName(mediaName);
  const thumbnailPath = '/wwwroot' + mediaData.values[0].value.src;
  await umbracoApi.dataType.createBlockGridWithAThumbnail(blockGridEditorName, contentElementTypeId, thumbnailPath);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.removeBlockThumbnail();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  await umbracoUi.dataType.doesBlockHaveNoThumbnailImage(elementTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can remove a not-found thumbnail from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaName = 'TestMedia';
  const imageId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const mediaData = await umbracoApi.media.getByName(mediaName);
  const thumbnailPath = '/wwwroot' + mediaData.values[0].value.src;
  await umbracoApi.dataType.createBlockGridWithAThumbnail(blockGridEditorName, contentElementTypeId, thumbnailPath);
  await umbracoApi.media.delete(imageId); // Make the thumbnail not found

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.removeNotFoundItem(thumbnailPath);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  await umbracoUi.dataType.doesBlockHaveNoThumbnailImage(elementTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

// This tests for regression issue: https://github.com/umbraco/Umbraco-CMS/issues/20962
test('only allow image file as a block thumbnail', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const notAllowedFileNames = ['Program.cs', 'appsettings.json', '.csproj'];
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.clickChooseThumbnailButton();
  
  // Assert
  for (const notAllowedFileName of notAllowedFileNames) {
    await umbracoUi.dataType.isModalMenuItemWithNameVisible(notAllowedFileName, false);
  }
});
