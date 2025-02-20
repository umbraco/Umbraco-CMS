import { test } from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

const listViewTypes = ['List View - Content', 'List View - Media'];
for (const listViewType of listViewTypes) {
  test.describe(`${listViewType} tests`, () => {
    let dataTypeDefaultData = null;
    let dataTypeData = null;

    test.beforeEach(async ({ umbracoUi, umbracoApi }) => {
      await umbracoUi.goToBackOffice();
      await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
      dataTypeDefaultData = await umbracoApi.dataType.getByName(listViewType);
    });

    test.afterEach(async ({ umbracoApi }) => {
      if (dataTypeDefaultData !== null) {
        await umbracoApi.dataType.update(dataTypeDefaultData.id,dataTypeDefaultData);
      }
    });

    test('can update page size', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const pageSizeValue = 5;
      const expectedDataTypeValues = {
        "alias": "pageSize",
        "value": pageSizeValue
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.enterPageSizeValue(pageSizeValue.toString());
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can update order direction', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const isAscending = listViewType == 'List View - Members' ? false : true;
      const orderDirectionValue = isAscending ? 'asc' : 'desc';
      const expectedDataTypeValues = {
        "alias": "orderDirection",
        "value": orderDirectionValue
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.chooseOrderDirection(isAscending);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can add column displayed', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      let columnData: string[];
      if (listViewType === 'List View - Media') {
        columnData = ['Document Type', 'TestDocumentType', 'sortOrder', 'Sort'];
        await umbracoApi.documentType.ensureNameNotExists(columnData[1]);
        await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(columnData[1]);
      } else {
        columnData = ['Media Type', 'Audio', 'sortOrder', 'Sort'];
      }

      const expectedIncludePropertiesValues = {
        "alias": columnData[2],
        "header": columnData[3],
        "isSystem": 1
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.addColumnDisplayed(columnData[0], columnData[1], columnData[2]);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      const includePropertiesData = dataTypeData.values.find(value => value.alias === "includeProperties");
      expect(includePropertiesData.value).toContainEqual(expectedIncludePropertiesValues);
    });

    test('can remove column displayed', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      let columnData: string[];
      if (listViewType === 'List View - Media') {
        columnData = ['Document Type', 'TestDocumentType', 'owner', 'Created by'];
        await umbracoApi.documentType.ensureNameNotExists(columnData[1]);
        await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(columnData[1]);
      } else {
        columnData = ['Media Type', 'Audio', 'owner', 'Created by'];
      }

      const removedDataTypeValues = [{
        "alias": "includeProperties",
        "value": [{
          "alias": columnData[2],
          "header": columnData[3],
          "isSystem": 1,
        }]
      }];

      // Remove all existing values and add a column displayed to remove
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      dataTypeData.values = removedDataTypeValues;
      await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.removeColumnDisplayed(columnData[2]);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toEqual([]);
    });

    test('can add layouts', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const layoutName = 'Extension Table Collection View';
      const layoutCollectionView = 'Umb.CollectionView.Extension.Table';
      const expectedIncludePropertiesValues = {
        "icon": "icon-list",
        "name": layoutName,
        "collectionView": layoutCollectionView,
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.addLayouts(layoutName);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      const includePropertiesData = dataTypeData.values.find(value => value.alias === "layouts");
      expect(includePropertiesData.value).toContainEqual(expectedIncludePropertiesValues);
    });

    test('can remove layouts', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      let layoutsData = 'Document Grid Collection View';
      if (listViewType === 'List View - Media') {
        layoutsData = 'Media Grid Collection View';
      }

      const removedDataTypeValues = [{
        "alias": "layouts",
        "value": [{
          "icon": "icon-thumbnails-small",
          "collectionView": layoutsData,
          "isSystem": true,
          "name": "Grid",
          "selected": true
        }]
      }];

      // Remove all existing values and add a layout to remove
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      dataTypeData.values = removedDataTypeValues;
      await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.removeLayouts(layoutsData);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toEqual([]);
    });

    test('can update order by', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const orderByValue = 'Last edited';
      const expectedDataTypeValues = {
        "alias": "orderBy",
        "value": "updateDate"
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.chooseOrderByValue(orderByValue);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    // Skip this test as currently there is no setting for bulk action permission
    test.skip('can update bulk action permission', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const bulkActionPermissionValue = 'Allow bulk trash';
      const expectedDataTypeValues = {
        "alias": "bulkActionPermissions",
        "value": {
          "allowBulkCopy": false,
          "allowBulkDelete": true,
          "allowBulkMove": false,
          "allowBulkPublish": false,
          "allowBulkUnpublish": false
        }
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.clickBulkActionPermissionsSliderByValue(bulkActionPermissionValue);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can update workspace view icon', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const iconValue = 'icon-activity';
      const expectedDataTypeValues = {
        "alias": "icon",
        "value": iconValue
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.clickSelectIconButton();
      await umbracoUi.dataType.chooseWorkspaceViewIconByValue(iconValue);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can update workspace view name', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const WorkspaceViewName = 'Test Content Name';
      const expectedDataTypeValues = {
        "alias": "tabName",
        "value": WorkspaceViewName
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.enterWorkspaceViewName(WorkspaceViewName);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can enable show content workspace view first', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const expectedDataTypeValues = {
        "alias": "showContentFirst",
        "value": true
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.clickShowContentWorkspaceViewFirstSlider();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    // Skip this test as there are no setting for infinite editor
    test.skip('can enable edit in infinite editor', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const expectedDataTypeValues = {
        "alias": "useInfiniteEditor",
        "value": true
      };

      // Act
      await umbracoUi.dataType.goToDataType(listViewType);
      await umbracoUi.dataType.clickEditInInfiniteEditorSlider();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      await umbracoUi.dataType.isSuccessNotificationVisible();
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });
  });
}
