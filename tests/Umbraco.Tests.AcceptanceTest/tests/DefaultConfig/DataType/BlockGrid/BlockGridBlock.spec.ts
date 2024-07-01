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

// Settings
test('can add a label to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const labelText = 'TestLabel';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText(labelText);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockGridEditorName, elementTypeId, labelText)).toBeTruthy();
});

test('can remove a label from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const labelText = 'TestLabel';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithLabel(blockGridEditorName, elementTypeId, labelText);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockGridEditorName, elementTypeId, labelText)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockLabelText();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockGridEditorName, elementTypeId, labelText)).toBeFalsy();
});

test('can open content model in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.openBlockContentModel();

  // Assert
  await umbracoUi.dataType.isElementWorkspaceOpenInBlock(elementTypeName);
});

test('can add a settings model to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.addBlockSettingsModel(secondElementName);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockGridEditorName, [settingsElementTypeId])).toBeTruthy();
});

test('can remove a settings model from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithContentAndSettingsElementType(blockGridEditorName, contentElementTypeId, settingsElementTypeId);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockGridEditorName, [settingsElementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockSettingsModel();
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockGridEditorName, [settingsElementTypeId])).toBeFalsy();
});

test('can enable allow in root from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickAllowInRootForBlock();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockHaveAllowInRootEnabled(blockGridEditorName, contentElementTypeId)).toBeTruthy();
});

test('can enable allow in areas from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickAllowInAreasForBlock();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockHaveAllowInAreasEnabled(blockGridEditorName, contentElementTypeId)).toBeTruthy();
});

test('can add a column span to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const columnSpan = [1];
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickShowResizeOptions();
  await umbracoUi.dataType.clickAvailableColumnSpans(columnSpan);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, columnSpan)).toBeTruthy();
});

test('can add multiple column spans to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const columnSpan = [1, 3, 6, 8];
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickShowResizeOptions();
  await umbracoUi.dataType.clickAvailableColumnSpans(columnSpan);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, columnSpan)).toBeTruthy();
});

test('can remove a column span from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const columnSpan = [4];
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithSizeOptions(blockGridEditorName, contentElementTypeId, columnSpan[0]);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickAvailableColumnSpans(columnSpan);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, [])).toBeTruthy();
});

test('can add min and max row span to a block', async ({page, umbracoApi, umbracoUi}) => {
// Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const rowSpanMin = 2;
  const rowSpanMax = 6;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickShowResizeOptions();
  await umbracoUi.dataType.enterMinRowSpan(rowSpanMin);
  await umbracoUi.dataType.enterMaxRowSpan(rowSpanMax);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockContainRowSpanOptions(blockGridEditorName, contentElementTypeId, rowSpanMin, rowSpanMax)).toBeTruthy();
});

test('can remove min and max row spans from a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const rowSpanMin = undefined;
  const rowSpanMax = undefined;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithSizeOptions(blockGridEditorName, contentElementTypeId, undefined, 2, 6);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterMinRowSpan(rowSpanMin);
  await umbracoUi.dataType.enterMaxRowSpan(rowSpanMax);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockContainRowSpanOptions(blockGridEditorName, contentElementTypeId, rowSpanMin, rowSpanMax)).toBeTruthy();
});

// AREAS

test('can update grid columns for areas for a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const griColumns = 6;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.enterGridColumnsForArea(griColumns);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockContainAreaGridColumns(blockGridEditorName, contentElementTypeId, griColumns)).toBeTruthy();
});

test('can add an area for a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.addAreaButton();

  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible()
  expect(await umbracoApi.dataType.doesBlockContainAreaWithAlias(blockGridEditorName, contentElementTypeId)).toBeTruthy();
});

test('can resize an area for a block', async ({page, umbracoApi, umbracoUi}) => {
// Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await page.pause();
});

test('can update alias an area for a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await page.pause();
});

test('can remove an area for a block', async ({page, umbracoApi, umbracoUi}) => {
// Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await page.pause();
});

test('can add multiple areas for a block', async ({page, umbracoApi, umbracoUi}) => {
// Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await page.pause();
});

test('can add create button label for an area in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await page.pause();
});

test('can remove create button label for an area in a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can add min allowed for an area in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await page.pause();
});

test('can remove min allowed for an area in a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can add add max allowed for an area in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await page.pause();
});

test('can add max allowed for an area in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await page.pause();
});

test('min can not be more than max an area in a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can add specified allowance for an area in a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await page.pause();
});

test('can update specified allowance for an area in a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can remove specified allowance for an area in a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can add multiple specified allowances for an area in a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can add specified allowance with min and max for an area in a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can remove min and max from specified allowance for an area in a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can add an element or what you can add to specified allowance for an area in a block', async ({page, umbracoApi, umbracoUi}) => {

});

// ADVANCDED
