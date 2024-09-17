import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const dataTypeName = 'List View - Media';
let dataTypeDefaultData = null;
const firstMediaFileName = 'FirstMediaFile';
const secondMediaFileName = 'SecondMediaFile';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.media.ensureNameNotExists(firstMediaFileName);
  await umbracoApi.media.createDefaultMediaFile(firstMediaFileName);
  await umbracoApi.media.ensureNameNotExists(secondMediaFileName);
  await umbracoApi.media.createDefaultMediaFile(secondMediaFileName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  if (dataTypeDefaultData !== null) {
    await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData);
  }
  await umbracoApi.media.ensureNameNotExists(firstMediaFileName);
  await umbracoApi.media.ensureNameNotExists(secondMediaFileName);
});

test('can change the the default sort order for the list in the media section', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const sortOrder = 'creator';
  const expectedMediaValues = await umbracoApi.media.getAllMediaNames(sortOrder);

  // Act
  await umbracoApi.dataType.updateListViewMediaDataType('orderBy', sortOrder);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);
  await umbracoUi.media.changeToListView();

  // Assert
  await umbracoUi.media.isMediaListViewVisible();
  await umbracoUi.media.doesMediaListNameValuesMatch(expectedMediaValues);
});

test('can change the the order direction for the list in the media section', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedMediaValues = await umbracoApi.media.getAllMediaNames('updateDate', 'Ascending');

  // Act
  await umbracoApi.dataType.updateListViewMediaDataType('orderDirection', 'asc');
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Assert
  await umbracoUi.media.isMediaGridViewVisible();
  await umbracoUi.media.doesMediaGridValuesMatch(expectedMediaValues);
  await umbracoUi.media.changeToListView();
  await umbracoUi.media.isMediaListViewVisible();
  await umbracoUi.media.doesMediaListNameValuesMatch(expectedMediaValues);
});

test('can add more columns to the list in the media section', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedColumns = ['Name', 'Last edited', 'Updated by', 'Size'];
  const updatedValue = [
    {"alias": "updateDate", "header": "Last edited", "isSystem": true},
    {"alias": "creator", "header": "Updated by", "isSystem": true},
    {"alias": "umbracoBytes", "header": "Size", "isSystem": 0}
  ];

  // Act
  await umbracoApi.dataType.updateListViewMediaDataType('includeProperties', updatedValue);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);
  await umbracoUi.media.changeToListView();

  // Assert
  await umbracoUi.media.isMediaListViewVisible();
  await umbracoUi.media.doesMediaListHeaderValuesMatch(expectedColumns);
});

test('can disable one view in the media section', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedValue = [
    {
      "name": "List",
      "collectionView": "Umb.CollectionView.Media.Table",
      "icon": "icon-list",
      "isSystem": true,
      "selected": true
    }
  ];
  
  // Act
  await umbracoApi.dataType.updateListViewMediaDataType('layouts', updatedValue);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Assert
  await umbracoUi.media.isViewBundleButtonVisible(false);
  await umbracoUi.media.isMediaListViewVisible();
  await umbracoUi.media.isMediaGridViewVisible(false);
});

test('can allow bulk trash in the media section', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedValue = {
    "allowBulkPublish": false,
    "allowBulkUnpublish": false,
    "allowBulkCopy": false,
    "allowBulkDelete": true,
    "allowBulkMove": false
  };
  
  // Act
  await umbracoApi.dataType.updateListViewMediaDataType('bulkActionPermissions', updatedValue);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);
  await umbracoUi.media.selectMediaByName(firstMediaFileName);
  await umbracoUi.media.selectMediaByName(secondMediaFileName);
  await umbracoUi.media.clickBulkTrashButton();
  await umbracoUi.media.clickConfirmTrashButton();

  // Assert
  await umbracoUi.media.isMediaItemVisibleInRecycleBin(firstMediaFileName);
  await umbracoUi.media.isMediaItemVisibleInRecycleBin(secondMediaFileName);
  expect(await umbracoApi.media.doesNameExist(firstMediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesNameExist(secondMediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(firstMediaFileName)).toBeTruthy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(secondMediaFileName)).toBeTruthy();
});

test('can allow bulk move in the media section', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaFolderName = 'Test Folder Name';
  const updatedValue = {
    "allowBulkPublish": false,
    "allowBulkUnpublish": false,
    "allowBulkCopy": false,
    "allowBulkDelete": false,
    "allowBulkMove": true
  };
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  const mediaFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  
  // Act
  await umbracoApi.dataType.updateListViewMediaDataType('bulkActionPermissions', updatedValue);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);
  await umbracoUi.media.selectMediaByName(firstMediaFileName);
  await umbracoUi.media.selectMediaByName(secondMediaFileName);
  await umbracoUi.media.clickBulkMoveToButton();
  await umbracoUi.media.clickCaretButtonForName('Media');
  await umbracoUi.media.clickModalTextByName(mediaFolderName);
  await umbracoUi.media.clickChooseModalButton();

  // Assert
  await umbracoUi.media.isSuccessNotificationVisible();
  expect(await umbracoApi.media.doesMediaItemHaveChildName(mediaFolderId, firstMediaFileName)).toBeTruthy();
  expect(await umbracoApi.media.doesMediaItemHaveChildName(mediaFolderId, secondMediaFileName)).toBeTruthy();
  
  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
});