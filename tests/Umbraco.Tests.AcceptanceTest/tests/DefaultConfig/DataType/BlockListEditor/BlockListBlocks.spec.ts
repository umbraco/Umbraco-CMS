import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const blockListEditorName = 'TestBlockListEditor';
const elementTypeName = 'BlockListElement';
const dataTypeName = 'Textstring';
const groupName = 'testGroup';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockListEditorName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockListEditorName);
});

test('can add a label to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const labelText = 'ThisIsALabel';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText(labelText);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, labelText)).toBeTruthy();
});

test('can update a label for a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const labelText = 'ThisIsALabel';
  const newLabelText = 'ThisIsANewLabel';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithEditorAppearance(blockListEditorName, elementTypeId, labelText);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, labelText)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText(newLabelText);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, newLabelText)).toBeTruthy();
});

test('can remove a label from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const labelText = 'ThisIsALabel';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithEditorAppearance(blockListEditorName, elementTypeId, labelText);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, labelText)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText("");
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockListEditorName, elementTypeId, "")).toBeTruthy();
});

test('can update overlay size for a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const overlaySize = 'medium';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithEditorAppearance(blockListEditorName, elementTypeId, "");

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.updateBlockOverlaySize(overlaySize);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  const blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].editorSize).toEqual(overlaySize);
});

test('can open content model in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickReferenceNodeLinkWithName(elementTypeName);

  // Assert
  await umbracoUi.dataType.isElementWorkspaceOpenInBlock(elementTypeName);
});

// TODO: Skip this test as it is impossible to remove a content model
test.skip('can remove a content model from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockContentModel();
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
});

test('can add a settings model to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.addBlockSettingsModel(secondElementName);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockListEditorName, [settingsElementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(secondElementName);
});

test('can remove a settings model from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithContentAndSettingsElementType(blockListEditorName, contentElementTypeId, settingsElementTypeId);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockListEditorName, [settingsElementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockSettingsModel();
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockListEditorName, [settingsElementTypeId])).toBeFalsy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(secondElementName);
});

test('can add a background color to a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backgroundColor = '#ff0000';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockBackgroundColor(backgroundColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  const blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual(backgroundColor);
});

test('can update a background color for a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backgroundColor = '#ff0000';
  const newBackgroundColor = '#ff4444';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, contentElementTypeId, backgroundColor);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual(backgroundColor);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockBackgroundColor(newBackgroundColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual(newBackgroundColor);
});

test('can delete a background color from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const backgroundColor = '#ff0000';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, contentElementTypeId, backgroundColor);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual(backgroundColor);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockBackgroundColor('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].backgroundColor).toEqual('');
});

test('can add a icon color to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const iconColor = '#ff0000';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockIconColor(iconColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  const blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].iconColor).toEqual(iconColor);
});

test('can update a icon color for a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const iconColor = '#ff0000';
  const newIconColor = '#ff4444';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, contentElementTypeId, "", iconColor);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].iconColor).toEqual(iconColor);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockIconColor(newIconColor);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].iconColor).toEqual(newIconColor);
});

test('can delete a icon color from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const iconColor = '#ff0000';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, contentElementTypeId, '', iconColor);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].iconColor).toEqual(iconColor);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.selectBlockIconColor('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
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
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, contentElementTypeId, '', '', encodedStylesheetPath);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  // Removes first stylesheet
  await umbracoUi.dataType.clickRemoveCustomStylesheetWithName(stylesheetName);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
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
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithCatalogueAppearance(blockListEditorName, contentElementTypeId, '', '', encodedStylesheetPath);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].stylesheet[0]).toEqual(encodedStylesheetPath);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickRemoveCustomStylesheetWithName(stylesheetName);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].stylesheet[0]).toBeUndefined();

  // Clean
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
});

test('can enable hide content editor in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickBlockListHideContentEditorButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  const blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].forceHideContentEditorInOverlay).toEqual(true);
});

test('can disable hide content editor in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithHideContentEditor(blockListEditorName, contentElementTypeId, true);
  let blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].forceHideContentEditorInOverlay).toEqual(true);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickBlockListHideContentEditorButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  blockData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(blockData.values[0].value[0].forceHideContentEditorInOverlay).toEqual(false);
});

// TODO: Thumbnails are not showing in the UI
test.skip('can add a thumbnail to a block ', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {

});

// TODO: Thumbnails are not showing in the UI
test.skip('can remove a thumbnail to a block ', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {

});
