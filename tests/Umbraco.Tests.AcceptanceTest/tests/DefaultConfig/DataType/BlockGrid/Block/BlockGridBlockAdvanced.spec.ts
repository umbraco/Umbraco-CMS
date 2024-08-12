import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const blockGridEditorName = 'TestBlockGridEditor';
const elementTypeName = 'BlockGridElement';
const dataTypeName = 'Textstring';
const groupName = 'testGroup';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockGridEditorName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockGridEditorName);
});

test.skip('can add a custom view to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab()
  await page.pause();
});

test.skip('can remove a custom view from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab()
  await page.pause();
});

test.skip('can add a custom stylesheet to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetName = 'TestStylesheet.css'
  const stylesheetPath = '/wwwroot/css/' + stylesheetName;
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab()
  await umbracoUi.dataType.chooseBlockCustomStylesheetWithName(stylesheetName);

  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainStylesheet(blockGridEditorName, contentElementTypeId, stylesheetPath)).toBeTruthy();

  // Clean
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
});

// TODO: should the stylesheet be saved as an array?
test.skip('can remove a custom stylesheet from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetName = 'TestStylesheet.css'
  const stylesheetPath = '/wwwroot/css/' + stylesheetName;
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAdvancedSettingsInBlock(blockGridEditorName, contentElementTypeId, undefined, stylesheetPath, undefined, undefined, undefined);
  await page.pause();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainStylesheet(blockGridEditorName, contentElementTypeId, stylesheetPath)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab()
});

test('can update overlay size in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const overlaySize = 'medium';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.updateBlockOverlaySize(overlaySize);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainOverlaySize(blockGridEditorName, contentElementTypeId, overlaySize)).toBeTruthy();
});

test('can enable inline editing mode in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.clickInlineEditingMode();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainInlineEditing(blockGridEditorName, contentElementTypeId, true)).toBeTruthy();
});

test('can disable inline editing mode in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAdvancedSettingsInBlock(blockGridEditorName, contentElementTypeId, undefined, undefined, 'small', true);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainInlineEditing(blockGridEditorName, contentElementTypeId, true)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab()
  await umbracoUi.dataType.clickInlineEditingMode()
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainInlineEditing(blockGridEditorName, contentElementTypeId, false)).toBeTruthy();
});

test('can enable hide content editor in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab()
  await umbracoUi.dataType.clickBlockGridHideContentEditorButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainHideContentEditor(blockGridEditorName, contentElementTypeId, true)).toBeTruthy();
});


test('can disable hide content editor in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAdvancedSettingsInBlock(blockGridEditorName, contentElementTypeId, undefined, undefined, 'small', false, true);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainHideContentEditor(blockGridEditorName, contentElementTypeId, true)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.clickBlockGridHideContentEditorButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainHideContentEditor(blockGridEditorName, contentElementTypeId, false)).toBeTruthy();
});

test('can add a background color to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const backGroundColor = '#000000';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.selectBlockBackgroundColor(backGroundColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainBackgroundColor(blockGridEditorName, contentElementTypeId, backGroundColor)).toBeTruthy();
});

test('can remove a background color to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const backGroundColor = '#000000';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithCatalogueAppearanceInBlock(blockGridEditorName, contentElementTypeId, backGroundColor);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainBackgroundColor(blockGridEditorName, contentElementTypeId, backGroundColor)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.selectBlockBackgroundColor('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainBackgroundColor(blockGridEditorName, contentElementTypeId, '')).toBeTruthy();
});

test('can add a icon color to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const iconColor = '#000000';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.selectBlockIconColor(iconColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainIconColor(blockGridEditorName, contentElementTypeId, iconColor)).toBeTruthy();
});

test('can remove a icon color from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const iconColor = '#000000';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithCatalogueAppearanceInBlock(blockGridEditorName, contentElementTypeId, '' , iconColor);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainIconColor(blockGridEditorName, contentElementTypeId, iconColor)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab();
  await umbracoUi.dataType.selectBlockIconColor('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainIconColor(blockGridEditorName, contentElementTypeId, '')).toBeTruthy();
});

test('can add a thumbnail to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaName = 'TestMedia';
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab()
  const mediaUrl = await umbracoApi.media.getMediaPathByName(mediaName);
  await umbracoUi.dataType.chooseBlockThumbnailWithPath(mediaUrl.fileName, mediaUrl.mediaPath);
  await page.pause();
});

test.skip('can remove a thumbnail from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAdvancedTab()
  await page.pause();
});
