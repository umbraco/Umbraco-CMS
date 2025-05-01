import { ConstantHelper, NotificationConstantHelper, test } from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

const listViewTypes = [
  {type: 'List View - Content', collectionViewGird: 'Umb.CollectionView.Document.Grid', collectionViewList: 'Umb.CollectionView.Document.Table'},
  {type: 'List View - Media', collectionViewGird: 'Umb.CollectionView.Media.Grid', collectionViewList: 'Umb.CollectionView.Media.Table'}
];
const editorAlias = 'Umbraco.ListView';
const editorUiAlias = 'Umb.PropertyEditorUi.Collection';
const customDataTypeName = 'Custom List View';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can update page size', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const pageSizeValue = 5;
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterPageSizeValue(pageSizeValue.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'pageSize', pageSizeValue)).toBeTruthy();
});

test('can update order direction', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const orderDirectionValue = 'asc';
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.chooseOrderDirection(true);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'orderDirection', orderDirectionValue)).toBeTruthy();
});

test('can add column displayed', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const columnData = ['Document Type', 'TestDocumentType', 'sortOrder', 'Sort'];
  await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(columnData[1]);
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.addColumnDisplayed(columnData[0], columnData[1], columnData[2]);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesListViewHaveProperty(customDataTypeName, columnData[3], columnData[2], 1)).toBeTruthy();
});

test('can remove column displayed', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  expect(await umbracoApi.dataType.doesListViewHaveProperty(customDataTypeName, 'Last edited', 'updateDate')).toBeTruthy();
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeColumnDisplayed('updateDate');
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesListViewHaveProperty(customDataTypeName, 'Last edited', 'updateDate')).toBeFalsy();
});

test('can add layouts', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const layoutName = 'Extension Table Collection View';
  const layoutCollectionView = 'Umb.CollectionView.Extension.Table';
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.addLayouts(layoutName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesListViewHaveLayout(customDataTypeName, layoutName, 'icon-list', layoutCollectionView)).toBeTruthy();
});

test('can remove layouts', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const layoutName = 'List';
  const layoutCollectionView = 'Umb.CollectionView.Document.Table';
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  expect(await umbracoApi.dataType.doesListViewHaveLayout(customDataTypeName, layoutName, 'icon-list', layoutCollectionView)).toBeTruthy();
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeLayouts(layoutCollectionView);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesListViewHaveLayout(customDataTypeName, layoutName, 'icon-list', layoutCollectionView)).toBeFalsy();
});

test('can update order by', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const orderByValue = 'Last edited';
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.chooseOrderByValue(orderByValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'orderBy', 'updateDate')).toBeTruthy();
});

test('can update workspace view icon', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const iconValue = 'icon-activity';
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickSelectIconButton();
  await umbracoUi.dataType.chooseWorkspaceViewIconByValue(iconValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'icon', iconValue)).toBeTruthy();
});

test('can update workspace view name', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const workspaceViewName = 'Test Content Name';
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterWorkspaceViewName(workspaceViewName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'tabName', workspaceViewName)).toBeTruthy();
});

test('can enable show content workspace view first', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickShowContentWorkspaceViewFirstToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'showContentFirst', true)).toBeTruthy();
});

for (const listView of listViewTypes) {
  test(`the default configuration of ${listView.type} is correct`, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.goToDataType(listView.type);

    // Assert
    await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.listViewSettings);
    await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.listViewSettings);
    await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
    await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(listView.type, 'pageSize', 100)).toBeTruthy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(listView.type, 'orderBy', 'updateDate')).toBeTruthy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(listView.type, 'orderDirection', 'desc')).toBeTruthy();
    expect(await umbracoApi.dataType.doesListViewHaveLayout(listView.type, 'Grid', 'icon-thumbnails-small', listView.collectionViewGird)).toBeTruthy();
    expect(await umbracoApi.dataType.doesListViewHaveLayout(listView.type, 'List', 'icon-list', listView.collectionViewList)).toBeTruthy();
    expect(await umbracoApi.dataType.doesListViewHaveProperty(listView.type, 'Last edited', 'updateDate')).toBeTruthy();
    expect(await umbracoApi.dataType.doesListViewHaveProperty(listView.type, 'Updated by', 'creator')).toBeTruthy();
    // TODO: Uncomment this when the front-end is ready. Currently there is no default icon for workspace view icon
    // expect(await umbracoApi.dataType.doesDataTypeHaveValue(listView.type, 'icon', 'icon-list')).toBeTruthy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(listView.type, 'tabName')).toBeFalsy();
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(listView.type, 'showContentFirst')).toBeFalsy();
  });
}
