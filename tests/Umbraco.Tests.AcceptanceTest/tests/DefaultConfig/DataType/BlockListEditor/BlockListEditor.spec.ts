import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const blockListEditorName = 'TestBlockListEditor';
const blockListLocatorName = 'Block List';
const blockListEditorAlias = 'Umbraco.BlockList';
const blockListEditorUiAlias = 'Umb.PropertyEditorUi.BlockList';

const elementTypeName = 'BlockListElement';
const dataTypeName = 'Textstring';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockListEditorName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockListEditorName);
});

test('can create a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuAtRoot();
  await umbracoUi.dataType.clickCreateButton();
  await umbracoUi.dataType.clickNewDataTypeThreeDotsButton();
  await umbracoUi.dataType.enterDataTypeName(blockListEditorName);
  await umbracoUi.dataType.clickChangeButton();
  await umbracoUi.dataType.selectPropertyEditorUIByName(blockListLocatorName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesNameExist(blockListEditorName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(dataTypeData.editorAlias).toBe(blockListEditorAlias);
  expect(dataTypeData.editorUiAlias).toBe(blockListEditorUiAlias);
});


test('can rename a block list editor', async ({page, umbracoApi, umbracoUi}) => {
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

test('can delete a block list editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockListId = await umbracoApi.dataType.createEmptyBlockListDataType(blockListEditorName);

  // Act
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.clickActionsMenuForDataType(blockListEditorName);
  await umbracoUi.dataType.clickDeleteExactButton();
  await umbracoUi.dataType.clickConfirmToDeleteButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  expect(await umbracoApi.dataType.doesExist(blockListId)).toBeFalsy();
  await umbracoUi.dataType.isTreeItemVisible(blockListEditorName, false);
});

test('can add a block to a block list editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName)
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDefaultElementType(elementTypeName, 'testGroup', dataTypeName, dataTypeData.id);
  const blockListId = await umbracoApi.dataType.createEmptyBlockListDataType(blockListEditorName);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await page.pause();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName)

});
