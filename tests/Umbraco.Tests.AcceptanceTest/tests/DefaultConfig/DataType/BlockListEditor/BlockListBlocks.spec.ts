import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const blockListEditorName = 'TestBlockListEditor';
const elementTypeName = 'BlockListElement';
const dataTypeName = 'Textstring';
const groupName = 'testGroup';
let elementTypeId = '';
let textStringData: any = null;

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  textStringData =  await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockListEditorName);
});

test('can add a label to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const labelText = 'ThisIsALabel';
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText(labelText);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, labelText)).toBeTruthy();
});

test('can update a label for a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const labelText = 'ThisIsALabel';
  const newLabelText = 'ThisIsANewLabel';
  await umbracoApi.dataType.createBlockListWithBlockWithEditorAppearance(blockListEditorName, elementTypeId, labelText);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, labelText)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText(newLabelText);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, newLabelText)).toBeTruthy();
});

test('can remove a label from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const labelText = 'ThisIsALabel';
  await umbracoApi.dataType.createBlockListWithBlockWithEditorAppearance(blockListEditorName, elementTypeId, labelText);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, labelText)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, '')).toBeTruthy();
});

test('can update overlay size for a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const overlaySize = 'medium';
  await umbracoApi.dataType.createBlockListWithBlockWithEditorAppearance(blockListEditorName, elementTypeId, '');

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.updateBlockOverlaySize(overlaySize);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  const blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].editorSize).toEqual(overlaySize);
});

test('can open content model in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickReferenceNodeLinkWithName(elementTypeName);

  // Assert
  await umbracoUi.dataType.isElementWorkspaceOpenInBlock(elementTypeName);
});

// Skip this test as it is impossible to remove a content model in front-end
test.skip('can remove a content model from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockContentModel();
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  // TODO: missing check that the content model is removed
});

test('can add a settings model to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.addBlockSettingsModel(secondElementName);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockListEditorName, [settingsElementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(secondElementName);
});

test('can remove a settings model from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithContentAndSettingsElementType(blockListEditorName, elementTypeId, settingsElementTypeId);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockListEditorName, [settingsElementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockSettingsModel();
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockListEditorName, [settingsElementTypeId])).toBeFalsy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(secondElementName);
});

test('can add a background color to a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backgroundColor = '#ff0000';
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockBackgroundColor(backgroundColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  const blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual(backgroundColor);
});

test('can update a background color for a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backgroundColor = '#ff0000';
  const newBackgroundColor = '#ff4444';
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, elementTypeId, backgroundColor);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual(backgroundColor);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockBackgroundColor(newBackgroundColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual(newBackgroundColor);
});

test('can delete a background color from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backgroundColor = '#ff0000';
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, elementTypeId, backgroundColor);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual(backgroundColor);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockBackgroundColor('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual('');
});

test('can add a icon color to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const iconColor = '#ff0000';
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockIconColor(iconColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  const blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].iconColor).toEqual(iconColor);
});

test('can update a icon color for a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const iconColor = '#ff0000';
  const newIconColor = '#ff4444';
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, elementTypeId, '', iconColor);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].iconColor).toEqual(iconColor);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockIconColor(newIconColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].iconColor).toEqual(newIconColor);
});

test('can delete a icon color from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const iconColor = '#ff0000';
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, elementTypeId, '', iconColor);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].iconColor).toEqual(iconColor);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockIconColor('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].iconColor).toEqual('');
});

// TODO: Remove skip when the front-end is ready. Currently it is not possible to update a stylesheet to a block
test.skip('can update a custom stylesheet for a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetName = 'TestStylesheet.css';
  const stylesheetPath = '/wwwroot/css/' + stylesheetName;
  const encodedStylesheetPath = await umbracoApi.stylesheet.encodeStylesheetPath(stylesheetPath);
  const secondStylesheetName = 'SecondStylesheet.css';
  const secondStylesheetPath = '/wwwroot/css/' + secondStylesheetName;
  const encodedSecondStylesheetPath = await umbracoApi.stylesheet.encodeStylesheetPath(secondStylesheetPath);
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  await umbracoApi.stylesheet.ensureNameNotExists(secondStylesheetName);
  await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);
  await umbracoApi.stylesheet.createDefaultStylesheet(secondStylesheetName);
  

  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, elementTypeId, '', '', encodedStylesheetPath);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  // Removes first stylesheet
  await umbracoUi.dataType.clickRemoveCustomStylesheetWithName(stylesheetName);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].stylesheet[0]).toEqual(encodedSecondStylesheetPath);

  // Clean
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  await umbracoApi.stylesheet.ensureNameNotExists(secondStylesheetName);
});

// TODO: Remove skip when the front-end is ready. Currently it is not possible to delete a stylesheet to a block
test.skip('can delete a custom stylesheet from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetName = 'TestStylesheet.css';
  const stylesheetPath = '/wwwroot/css/' + stylesheetName;
  const encodedStylesheetPath = await umbracoApi.stylesheet.encodeStylesheetPath(stylesheetPath);
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);
  

  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, elementTypeId, '', '', encodedStylesheetPath);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].stylesheet[0]).toEqual(encodedStylesheetPath);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickRemoveCustomStylesheetWithName(stylesheetName);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].stylesheet[0]).toBeUndefined();

  // Clean
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
});

test('can enable hide content editor in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickBlockListHideContentEditorButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  const blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].forceHideContentEditorInOverlay).toEqual(true);
});

test('can disable hide content editor in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockListWithBlockWithHideContentEditor(blockListEditorName, elementTypeId, true);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].forceHideContentEditorInOverlay).toEqual(true);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickBlockListHideContentEditorButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].forceHideContentEditorInOverlay).toEqual(false);
});

test('can add a thumbnail to a block', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaName = 'TestMedia';
  const mediaId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);
  const mediaUrl = await umbracoApi.media.getFullMediaUrl(mediaId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.dataType.chooseBlockThumbnailWithPath(mediaUrl);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  await umbracoUi.dataType.doesBlockHaveThumbnailImage(elementTypeName, mediaUrl);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can remove a thumbnail from a block ', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
// Arrange
  const mediaName = 'TestMedia';
  await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const mediaData = await umbracoApi.media.getByName(mediaName);
  const thumbnailPath = '/wwwroot' + mediaData.values[0].value.src;
  await umbracoApi.dataType.createBlockListWithAThumbnail(blockListEditorName, elementTypeId, thumbnailPath);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockThumbnail();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  await umbracoUi.dataType.doesBlockHaveNoThumbnailImage(elementTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can remove a not-found thumbnail from a block ', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
// Arrange
  const mediaName = 'TestMedia';
  const imageId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const mediaData = await umbracoApi.media.getByName(mediaName);
  const thumbnailPath = '/wwwroot' + mediaData.values[0].value.src;
  await umbracoApi.dataType.createBlockListWithAThumbnail(blockListEditorName, elementTypeId, thumbnailPath);
  await umbracoApi.media.delete(imageId); // Make the thumbnail not found

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
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
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickChooseThumbnailButton();

  // Assert
  for (const notAllowedFileName of notAllowedFileNames) {
    await umbracoUi.dataType.isModalMenuItemWithNameVisible(notAllowedFileName, false);
  }
});
