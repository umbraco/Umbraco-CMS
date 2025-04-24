import {test} from "@umbraco/playwright-testhelpers";
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

test('can create a block grid editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  //  Arrange
  const blockGridLocatorName = 'Block Grid';
  const blockGridEditorAlias = 'Umbraco.BlockGrid';
  const blockGridEditorUiAlias = 'Umb.PropertyEditorUi.BlockGrid';

  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickActionsMenuCreateButton();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(blockGridEditorName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(blockGridLocatorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(blockGridEditorName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(blockGridEditorName);
  expect(dataTypeData.editorAlias).toBe(blockGridEditorAlias);
  expect(dataTypeData.editorUiAlias).toBe(blockGridEditorUiAlias);
});

test('can rename a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'BlockListEditorTest';
  await umbracoApi.dataType.createEmptyBlockGrid(wrongName);

  // Act
  await umbracoUi.dataType.goToDataType(wrongName);
  await umbracoUi.dataType.enterDataTypeName(blockGridEditorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesNameExist(blockGridEditorName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(wrongName)).toBeFalsy();
});

test('can delete a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockGridId = await umbracoApi.dataType.createEmptyBlockGrid(blockGridEditorName);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(blockGridEditorName);
  await umbracoUi.dataType.clickDeleteAndConfirmButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesExist(blockGridId)).toBeFalsy();
  await umbracoUi.dataType.isTreeItemVisible(blockGridEditorName, false);
});

test('can add a block to a block grid editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createEmptyBlockGrid(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickAddBlockButton();
  await umbracoUi.dataType.clickLabelWithName(elementTypeName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can add multiple blocks to a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondElementName = 'SecondBlockGridElement';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickAddBlockButton();
  await umbracoUi.dataType.clickLabelWithName(secondElementName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId, secondElementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeId);
});

test('can remove a block from a block grid editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickRemoveBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeFalsy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can add a block to a group in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createEmptyBlockGrid(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickAddGroupButton();
  await umbracoUi.dataType.enterGroupName(groupName);
  await umbracoUi.dataType.clickAddBlockButton(1);
  await umbracoUi.dataType.clickLabelWithName(elementTypeName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockGridGroupContainCorrectBlocks(blockGridEditorName, groupName, [elementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can add multiple blocks to a group in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondElementName = 'SecondBlockGridElement';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlockInAGroup(blockGridEditorName, elementTypeId, groupName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickAddBlockButton(1);
  await umbracoUi.dataType.clickLabelWithName(secondElementName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockGridGroupContainCorrectBlocks(blockGridEditorName, groupName, [elementTypeId, secondElementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementName);
});

test('can remove a block in a group from a block grid editor', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlockInAGroup(blockGridEditorName, elementTypeId, groupName);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickRemoveBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeFalsy();
});

test('can move a block from a group to another group in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondGroupName = 'MoveToHereGroup';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlockInAGroup(blockGridEditorName, elementTypeId, groupName);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickAddGroupButton();
  await umbracoUi.dataType.enterGroupName(secondGroupName, 1);
  // Drag and Drop
  const dragFromLocator = await umbracoUi.dataType.getLinkWithName(elementTypeName);
  const dragToLocator = await umbracoUi.dataType.getAddButtonInGroupWithName(secondGroupName);
  await umbracoUi.dataType.dragAndDrop(dragFromLocator, dragToLocator, -10, 0, 10);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockGridGroupContainCorrectBlocks(blockGridEditorName, secondGroupName, [elementTypeId])).toBeTruthy();
  expect(await umbracoApi.dataType.doesBlockGridGroupContainCorrectBlocks(blockGridEditorName, groupName, [elementTypeId])).toBeFalsy();
});

// TODO: When deleting a group should there not be a confirmation button? and should the block be moved another group when the group it was in is deleted?
test.skip('can delete a group in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlockInAGroup(blockGridEditorName, elementTypeId, groupName);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
});

test('can add a min and max amount to a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 1;
  const maxAmount = 2;
  await umbracoApi.dataType.createEmptyBlockGrid(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterMinAmount(minAmount.toString());
  await umbracoUi.dataType.enterMaxAmount(maxAmount.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  const dataTypeData = await umbracoApi.dataType.getByName(blockGridEditorName);
  expect(dataTypeData.values[0].value.min).toBe(minAmount);
  expect(dataTypeData.values[0].value.max).toBe(maxAmount);
});

test('max can not be less than min in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 2;
  const oldMaxAmount = 2;
  const newMaxAmount = 1;
  await umbracoApi.dataType.createBlockGridWithMinAndMaxAmount(blockGridEditorName, minAmount, oldMaxAmount);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterMaxAmount(newMaxAmount.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible(false);
  await umbracoUi.dataType.doesAmountContainErrorMessageWithText('The low value must not be exceed the high value');
  const dataTypeData = await umbracoApi.dataType.getByName(blockGridEditorName);
  expect(dataTypeData.values[0].value.min).toBe(minAmount);
  // The max value should not be updated
  expect(dataTypeData.values[0].value.max).toBe(oldMaxAmount);
});

test('can enable live editing mode in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createEmptyBlockGrid(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickLiveEditingMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.isLiveEditingModeEnabledForBlockEditor(blockGridEditorName, true)).toBeTruthy();
});

test('can disable live editing mode in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockGridWithLiveEditingMode(blockGridEditorName, true);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickLiveEditingMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.isLiveEditingModeEnabledForBlockEditor(blockGridEditorName, false)).toBeTruthy();
});

test('can add editor width in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const propertyEditorWidth = '100%';
  await umbracoApi.dataType.createEmptyBlockGrid(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterEditorWidth(propertyEditorWidth);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockGridEditorName, propertyEditorWidth)).toBeTruthy();
});

test('can remove editor width in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const propertyEditorWidth = '100%';
  await umbracoApi.dataType.createBlockGridWithPropertyEditorWidth(blockGridEditorName, propertyEditorWidth);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterEditorWidth('');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockGridEditorName, propertyEditorWidth)).toBeFalsy();
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockGridEditorName, '')).toBeTruthy();
});

test('can add a create button label in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const createButtonLabel = 'Create Block';
  await umbracoApi.dataType.createEmptyBlockGrid(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterCreateButtonLabel(createButtonLabel);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockGridContainCreateButtonLabel(blockGridEditorName, createButtonLabel)).toBeTruthy();
});

test('can remove a create button label in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const createButtonLabel = 'Create Block';
  await umbracoApi.dataType.createBlockGridWithCreateButtonLabel(blockGridEditorName, createButtonLabel);
  expect(await umbracoApi.dataType.doesBlockGridContainCreateButtonLabel(blockGridEditorName, createButtonLabel)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterCreateButtonLabel('');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockGridContainCreateButtonLabel(blockGridEditorName, createButtonLabel)).toBeFalsy();
  expect(await umbracoApi.dataType.doesBlockGridContainCreateButtonLabel(blockGridEditorName, '')).toBeTruthy();
});

test('can update grid columns in a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const gridColumns = 3;
  await umbracoApi.dataType.createEmptyBlockGrid(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterGridColumns(gridColumns);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockGridContainGridColumns(blockGridEditorName, gridColumns)).toBeTruthy();
});

// TODO: wait until fixed by frontend, currently you are able to insert multiple stylesheets
test.skip('can add a stylesheet a block grid editor', async ({umbracoApi, umbracoUi}) => {
});

test.skip('can remove a stylesheet in a block grid editor', async ({umbracoApi, umbracoUi}) => {
});
