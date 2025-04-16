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

test('can create a block list editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockListLocatorName = 'Block List';
  const blockListEditorAlias = 'Umbraco.BlockList';
  const blockListEditorUiAlias = 'Umb.PropertyEditorUi.BlockList';

  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickActionsMenuCreateButton();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(blockListEditorName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(blockListLocatorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesNameExist(blockListEditorName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(dataTypeData.editorAlias).toBe(blockListEditorAlias);
  expect(dataTypeData.editorUiAlias).toBe(blockListEditorUiAlias);
});

test('can rename a block list editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'BlockGridEditorTest';
  await umbracoApi.dataType.createEmptyBlockListDataType(wrongName);

  // Act
  await umbracoUi.dataType.goToDataType(wrongName);
  await umbracoUi.dataType.enterDataTypeName(blockListEditorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesNameExist(blockListEditorName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(wrongName)).toBeFalsy();
});

test('can delete a block list editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockListId = await umbracoApi.dataType.createEmptyBlockListDataType(blockListEditorName);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(blockListEditorName);
  await umbracoUi.dataType.clickDeleteButton();
  await umbracoUi.dataType.clickConfirmToDeleteButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesExist(blockListId)).toBeFalsy();
  await umbracoUi.dataType.isTreeItemVisible(blockListEditorName, false);
});

test('can add a block to a block list editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, 'testGroup', dataTypeName, textStringData.id);
  await umbracoApi.dataType.createEmptyBlockListDataType(blockListEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.clickAddBlockButton();
  await umbracoUi.dataType.clickLabelWithName(elementTypeName);
  await umbracoUi.dataType.clickChooseButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockListEditorName, [elementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can add multiple blocks to a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondElementTypeName = 'SecondBlockListElement';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.clickAddBlockButton();
  await umbracoUi.dataType.clickLabelWithName(secondElementTypeName);
  await umbracoUi.dataType.clickChooseButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockListEditorName, [elementTypeId, secondElementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
});

test('can remove a block from a block list editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.clickRemoveBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockListEditorName, [elementTypeId])).toBeFalsy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can add a min and max amount to a block list editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 1;
  const maxAmount = 2;
  await umbracoApi.dataType.createEmptyBlockListDataType(blockListEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.enterMinAmount(minAmount.toString());
  await umbracoUi.dataType.enterMaxAmount(maxAmount.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  const dataTypeData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(dataTypeData.values[0].value.min).toBe(minAmount);
  expect(dataTypeData.values[0].value.max).toBe(maxAmount);
});

test('max can not be less than min', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 2;
  const oldMaxAmount = 2;
  const newMaxAmount = 1;
  await umbracoApi.dataType.createBlockListDataTypeWithMinAndMaxAmount(blockListEditorName, minAmount, oldMaxAmount);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.enterMaxAmount(newMaxAmount.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible(false);
  const dataTypeData = await umbracoApi.dataType.getByName(blockListEditorName);
  await umbracoUi.dataType.doesAmountContainErrorMessageWithText('The low value must not be exceed the high value');
  expect(dataTypeData.values[0].value.min).toBe(minAmount);
  // The max value should not be updated
  expect(dataTypeData.values[0].value.max).toBe(oldMaxAmount);
});

test('can enable single block mode', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockListDataTypeWithSingleBlockMode(blockListEditorName, false);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.clickSingleBlockMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.isSingleBlockModeEnabledForBlockList(blockListEditorName, true)).toBeTruthy();
});

test('can disable single block mode', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockListDataTypeWithSingleBlockMode(blockListEditorName, true);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.clickSingleBlockMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.isSingleBlockModeEnabledForBlockList(blockListEditorName, false)).toBeTruthy();
});

test('can enable live editing mode', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockListDataTypeWithLiveEditingMode(blockListEditorName, false);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.clickLiveEditingMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.isLiveEditingModeEnabledForBlockEditor(blockListEditorName, true)).toBeTruthy();
});

test('can disable live editing mode', async ({umbracoApi, umbracoUi}) => {
// Arrange
  await umbracoApi.dataType.createBlockListDataTypeWithLiveEditingMode(blockListEditorName, true);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.clickLiveEditingMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.isLiveEditingModeEnabledForBlockEditor(blockListEditorName, false)).toBeTruthy();
});

test('can enable inline editing mode', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingMode(blockListEditorName, false);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.clickInlineEditingMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.isInlineEditingModeEnabledForBlockList(blockListEditorName, true)).toBeTruthy();
});

test('can disable inline editing mode', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingMode(blockListEditorName, true);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.clickInlineEditingMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.isInlineEditingModeEnabledForBlockList(blockListEditorName, false)).toBeTruthy();
});

test('can add a property editor width', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const propertyWidth = '50%';
  await umbracoApi.dataType.createEmptyBlockListDataType(blockListEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.enterPropertyEditorWidth(propertyWidth);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockListEditorName, propertyWidth)).toBeTruthy();
});

test('can update a property editor width', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const oldPropertyWidth = '50%';
  const newPropertyWidth = '100%';
  await umbracoApi.dataType.createBlockListDataTypeWithPropertyEditorWidth(blockListEditorName, oldPropertyWidth);
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockListEditorName, oldPropertyWidth)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.enterPropertyEditorWidth(newPropertyWidth);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockListEditorName, newPropertyWidth)).toBeTruthy();
});

test('can remove a property editor width', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const propertyWidth = '50%';
  await umbracoApi.dataType.createBlockListDataTypeWithPropertyEditorWidth(blockListEditorName, propertyWidth);
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockListEditorName, propertyWidth)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.enterPropertyEditorWidth('');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockListEditorName, '')).toBeTruthy();
});
