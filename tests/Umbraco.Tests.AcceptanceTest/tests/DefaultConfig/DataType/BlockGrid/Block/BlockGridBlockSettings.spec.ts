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

test('can add a label to a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const labelText = 'TestLabel';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText(labelText);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockGridEditorName, elementTypeId, labelText)).toBeTruthy();
});

test('can remove a label from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const labelText = 'TestLabel';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithEditorAppearance(blockGridEditorName, elementTypeId, labelText);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockGridEditorName, elementTypeId, labelText)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText('');
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainLabel(blockGridEditorName, elementTypeId, labelText)).toBeFalsy();
});

test('can open content model in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickReferenceNodeLinkWithName(elementTypeName);

  // Assert
  await umbracoUi.dataType.isElementWorkspaceOpenInBlock(elementTypeName);
});

test('can add a settings model to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.addBlockSettingsModel(secondElementName);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockGridEditorName, [settingsElementTypeId])).toBeTruthy();
});

test('can remove a settings model from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementName = 'SecondElementTest';
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithContentAndSettingsElementType(blockGridEditorName, contentElementTypeId, settingsElementTypeId);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockGridEditorName, [settingsElementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.removeBlockSettingsModel();
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithSettingsTypeIds(blockGridEditorName, [settingsElementTypeId])).toBeFalsy();
});

test('can enable allow in root from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickAllowInRootForBlock();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockHaveAllowInRootEnabled(blockGridEditorName, contentElementTypeId)).toBeTruthy();
});

test('can enable allow in areas from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickAllowInAreasForBlock();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockHaveAllowInAreasEnabled(blockGridEditorName, contentElementTypeId)).toBeTruthy();
});

test('can add a column span to a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const columnSpan = [1];
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickShowResizeOptions();
  await umbracoUi.dataType.clickAvailableColumnSpans(columnSpan);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, columnSpan)).toBeTruthy();
});

test('can add multiple column spans to a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const columnSpan = [1, 3, 6, 8];
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickShowResizeOptions();
  await umbracoUi.dataType.clickAvailableColumnSpans(columnSpan);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, columnSpan)).toBeTruthy();
});

test('can remove a column span from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const columnSpan = [4];
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithSizeOptions(blockGridEditorName, contentElementTypeId, columnSpan[0]);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, columnSpan)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickAvailableColumnSpans(columnSpan);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainColumnSpanOptions(blockGridEditorName, contentElementTypeId, [])).toBeTruthy();
});

test('can add min and max row span to a block', async ({umbracoApi, umbracoUi}) => {
// Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const rowSpanMin = 2;
  const rowSpanMax = 6;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickShowResizeOptions();
  await umbracoUi.dataType.enterMinRowSpan(rowSpanMin);
  await umbracoUi.dataType.enterMaxRowSpan(rowSpanMax);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainRowSpanOptions(blockGridEditorName, contentElementTypeId, rowSpanMin, rowSpanMax)).toBeTruthy();
});

test('can remove min and max row spans from a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const rowSpanMin = undefined;
  const rowSpanMax = undefined;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithSizeOptions(blockGridEditorName, contentElementTypeId, undefined, 2, 6);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainRowSpanOptions(blockGridEditorName, contentElementTypeId, 2, 6)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterMinRowSpan(rowSpanMin);
  await umbracoUi.dataType.enterMaxRowSpan(rowSpanMax);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainRowSpanOptions(blockGridEditorName, contentElementTypeId, rowSpanMin, rowSpanMax)).toBeTruthy();
});
