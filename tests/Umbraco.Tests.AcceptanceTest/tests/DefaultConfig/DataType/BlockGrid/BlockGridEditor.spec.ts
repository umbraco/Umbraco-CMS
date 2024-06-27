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

test('can create a block grid editor', async ({umbracoApi, umbracoUi}) => {
  //Arrange
  const blockGridLocatorName = 'Block Grid';
  const blockGridEditorAlias = 'Umbraco.BlockGrid';
  const blockGridEditorUiAlias = 'Umb.PropertyEditorUi.BlockGrid';

  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickCreateButton();
  await umbracoUi.dataType.clickNewDataTypeThreeDotsButton();
  await umbracoUi.dataType.enterDataTypeName(blockGridEditorName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(blockGridLocatorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesNameExist(blockGridEditorName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(blockGridEditorName);
  expect(dataTypeData.editorAlias).toBe(blockGridEditorAlias);
  expect(dataTypeData.editorUiAlias).toBe(blockGridEditorUiAlias);
});

test('can rename a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'BlockListEditorTest';
  await umbracoApi.dataType.createEmptyBlockGridDataType(wrongName);

  // Act
  await umbracoUi.dataType.goToDataType(wrongName);
  await umbracoUi.dataType.enterDataTypeName(blockGridEditorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesNameExist(blockGridEditorName)).toBeTruthy();
  expect(await umbracoApi.dataType.doesNameExist(wrongName)).toBeFalsy();
});

test('can delete a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockGridId = await umbracoApi.dataType.createEmptyBlockGridDataType(blockGridEditorName);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(blockGridEditorName);
  await umbracoUi.dataType.clickDeleteExactButton();
  await umbracoUi.dataType.clickConfirmToDeleteButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesExist(blockGridId)).toBeFalsy();
  await umbracoUi.dataType.isTreeItemVisible(blockGridEditorName, false);
});

test('can add a block to a block grid editor', async ({page ,umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createEmptyBlockGridDataType(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickAddBlockButton();
  await umbracoUi.dataType.clickLabelWithName(elementTypeName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can add multiple blocks to a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondElementName = 'SecondBlockGridElement';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickAddBlockButton();
  await umbracoUi.dataType.clickLabelWithName(secondElementName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId, secondElementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeId);
});

test('can remove a block from a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlock(blockGridEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickRemoveBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeFalsy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName)
});

test('can add a block to a group in a block grid editor', async ({page,umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createEmptyBlockGridDataType(blockGridEditorName);

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
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockGridGroupContainCorrectBlocks(blockGridEditorName, groupName, [elementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can add multiple blocks to a group in a block grid editor', async ({page,umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondElementName = 'SecondBlockGridElement';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  const secondElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlockInAGroup(blockGridEditorName, elementTypeId, groupName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickAddBlockButton(1);
  await umbracoUi.dataType.clickLabelWithName(secondElementName);
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockGridGroupContainCorrectBlocks(blockGridEditorName, groupName, [elementTypeId, secondElementTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementName);
});

test('can delete a block in a group from a block grid editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlockInAGroup(blockGridEditorName, elementTypeId, groupName);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickRemoveBlockWithName(elementTypeName);
  await umbracoUi.dataType.clickConfirmRemoveButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeFalsy();
});


// THIS TEST IS CURRENTLY FLAKY.
test('can move a block from a group to another group in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondGroupName = 'MoveToHereGroup';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlockInAGroup(blockGridEditorName, elementTypeId, groupName);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickAddGroupButton();
  await umbracoUi.dataType.enterGroupName(secondGroupName,1);
  // Drag and Drop
  const dragFromLocator = page.getByRole('link', {name: elementTypeName});
  const dragToLocator =   page.locator('.group').filter({hasText: secondGroupName}).locator('#add-button');
  await umbracoUi.dataType.dragAndDrop(dragFromLocator, dragToLocator, -10, 0, 10);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockGridGroupContainCorrectBlocks(blockGridEditorName, secondGroupName, [elementTypeId])).toBeTruthy();
  expect(await umbracoApi.dataType.doesBlockGridGroupContainCorrectBlocks(blockGridEditorName, groupName, [elementTypeId])).toBeFalsy();
});


// When deleting a group should there not be a confirmation button? and should the block be moved another group when the group it was in is deleted?
test.skip('can delete a group in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridDataTypeWithABlockInAGroup(blockGridEditorName, elementTypeId, groupName);
  expect(await umbracoApi.dataType.doesBlockEditorContainBlocksWithContentTypeIds(blockGridEditorName, [elementTypeId])).toBeTruthy();


  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await page.pause();
});

test('can add a min and max amount to a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createEmptyBlockGridDataType(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterMinAmount('1');
  await umbracoUi.dataType.enterMaxAmount('2');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  const dataTypeData = await umbracoApi.dataType.getByName(blockGridEditorName);
  expect(dataTypeData.values[0].value.min).toBe(1);
  expect(dataTypeData.values[0].value.max).toBe(2);
});

test('max can not be less than min in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockGridDataTypeWithMinAndMaxAmount(blockGridEditorName, 2, 2);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterMaxAmount('1');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible(false);
  const dataTypeData = await umbracoApi.dataType.getByName(blockGridEditorName);
  expect(await umbracoUi.dataType.doesAmountContainErrorMessageWitText('The low value must not be exceed the high value'));
  expect(dataTypeData.values[0].value.min).toBe(2);
  // The max value should not be updated
  expect(dataTypeData.values[0].value.max).toBe(2);
});

test('can enable live editing mode in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createEmptyBlockGridDataType(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickLiveEditingMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoApi.dataType.isLiveEditingModeEnabledForBlockEditor(blockGridEditorName, true);
});

test('can disable live editing mode in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createBlockGridDataTypeWithLiveEditingMode(blockGridEditorName, true);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.clickLiveEditingMode();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoApi.dataType.isLiveEditingModeEnabledForBlockEditor(blockGridEditorName, false);
});

test('can add editor width in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const propertyEditorWidth = '100%'
  await umbracoApi.dataType.createEmptyBlockGridDataType(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterEditorWidth(propertyEditorWidth);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockGridEditorName, propertyEditorWidth)).toBeTruthy();
});

test('can remove editor width in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const propertyEditorWidth = '100%'
  await umbracoApi.dataType.createBlockGridDataTypeWithPropertyEditorWidth(blockGridEditorName, propertyEditorWidth);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterEditorWidth('');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockGridEditorName, propertyEditorWidth)).toBeFalsy();
  expect(await umbracoApi.dataType.doesMaxPropertyContainWidthForBlockEditor(blockGridEditorName, '')).toBeTruthy();
});

test('can add a create button label in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const createButtonLabel = 'Create Block'
  await umbracoApi.dataType.createEmptyBlockGridDataType(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterCreateButtonLabel(createButtonLabel);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockGridDataTypeContainCreateButtonLabel(blockGridEditorName, createButtonLabel)).toBeTruthy();
});

test('can remove a create button label in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const createButtonLabel = 'Create Block'
  await umbracoApi.dataType.createBlockGridDataTypeWithCreateButtonLabel(blockGridEditorName, createButtonLabel);
  expect(await umbracoApi.dataType.doesBlockGridDataTypeContainCreateButtonLabel(blockGridEditorName, createButtonLabel)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterCreateButtonLabel('');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockGridDataTypeContainCreateButtonLabel(blockGridEditorName, createButtonLabel)).toBeFalsy();
  expect(await umbracoApi.dataType.doesBlockGridDataTypeContainCreateButtonLabel(blockGridEditorName, '')).toBeTruthy();
});

test('can update grid columns in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const gridColumns = 3;
  await umbracoApi.dataType.createEmptyBlockGridDataType(blockGridEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.enterGridColumns(gridColumns);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesBlockGridDataTypeContainGridColumns(blockGridEditorName, gridColumns)).toBeTruthy();
});

test('can add a layout stylesheet a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
});

test('can delete a layout stylesheet in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
});





