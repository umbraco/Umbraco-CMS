import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const blockListEditorName = 'TestBlockListEditor';
const blockListLocatorName = 'Block List';
const blockListEditorAlias = 'Umbraco.BlockList';
const blockListEditorUiAlias = 'Umb.PropertyEditorUi.BlockList';

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

test('can delete a block list editor', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  console.log(await umbracoApi.dataType.createEmptyBlockListDataType(blockListEditorName));

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await page.pause();
});
