import { test } from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

// TODO: Added List View - Members to the list when the front-end is ready
//const listViewTypes = ['List View - Content', 'List View - Media', 'List View - Members'];
const listViewTypes = ['List View - Content', 'List View - Media'];
for (const listViewType of listViewTypes) {
  test.describe(`${listViewType} tests`, () => {
    let dataTypeDefaultData = null;
    let dataTypeData = null;

    test.beforeEach(async ({ umbracoUi, umbracoApi }) => {
      await umbracoUi.goToBackOffice();
      await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
      dataTypeDefaultData = await umbracoApi.dataType.getByName(listViewType);
      await umbracoUi.dataType.goToDataType(listViewType);
    });

    test.afterEach(async ({ umbracoApi }) => {
      if (dataTypeDefaultData !== null) {
        await umbracoApi.dataType.update(dataTypeDefaultData.id,dataTypeDefaultData);
      }
    });

    test(`can update Page Size`, async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const pageSizeValue = 5;
      const expectedDataTypeValues = {
        alias: "pageSize",
        value: pageSizeValue,
      };

      // Act
      await umbracoUi.dataType.enterPageSizeValue(pageSizeValue.toString());
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test(`can update Order Direction`, async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const isAscending = listViewType == 'List View - Members' ? false : true;
      const orderDirectionValue = isAscending ? 'asc' : 'desc';
      const expectedDataTypeValues = {
        alias: "orderDirection",
        value: orderDirectionValue,
      };

      // Act
      await umbracoUi.dataType.chooseOrderDirection(isAscending);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    //TODO: Uncomment when the frontend works
    test.skip(`can add Column Displayed`, async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const columnName = 'Document Type';
      const expectedIncludePropertiesValues = {
        alias: "contentTypeAlias",
        header: "Document Type",
        isSystem: true,
      };

      // Act
      await umbracoUi.dataType.addColumnDisplayed(columnName);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      const includePropertiesData = dataTypeData.values.find(value => value.alias === "includeProperties");
      expect(includePropertiesData.value).toContainEqual(expectedIncludePropertiesValues);
    });

    //TODO: Uncomment when the frontend works
    test.skip(`can remove Column Displayed`, async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const columnName = 'Last edited';
      const expectedIncludePropertiesValues = {
        alias: "updateDate",
        header: "Last edited",
        isSystem: 1,
      };

      // Act
      await umbracoUi.dataType.removeColumnDisplayed(columnName);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      const includePropertiesData = dataTypeData.values.find(value => value.alias === "includeProperties");
      expect(includePropertiesData.value).not.toContainEqual(expectedIncludePropertiesValues);
    });

    test(`can update Order By`, async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const orderByValue = 'Last edited';
      const expectedDataTypeValues = {
        alias: "orderBy",
        value: "updateDate",
      };

      // Act
      await umbracoUi.dataType.chooseOrderByValue(orderByValue);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    // The output is currently not the same
    test.skip(`can update Bulk Action Permission`, async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const bulkActionPermissionValue = 'Allow bulk copy (content only)';
      const expectedDataTypeValues = {
        alias: "bulkActionPermissions",
        value: {
          allowBulkPublish: false,
          allowBulkUnpublish: false,
          allowBulkCopy: true,
          allowBulkDelete: false,
          allowBulkMove: false,
        },
      };

      // Act
      await umbracoUi.dataType.clickBulkActionPermissionsSliderByValue(bulkActionPermissionValue);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test(`can update Content App Icon`, async ({ page, umbracoApi, umbracoUi }) => {
      // Arrange
      const iconValue = 'icon-activity';
      const expectedDataTypeValues = {
        "alias": "icon",
        "value": iconValue
      };

      // Act
      await umbracoUi.dataType.clickContentAppIconButton();
      // TODO: Uncomment one of the possible ways to select the icon. when the helpers are fixed
      // await umbracoUi.dataType.clickLabelWithName(iconValue, true);
      // await umbracoUi.dataType.chooseContentAppIconByValue(iconValue);
      await page.getByLabel(iconValue).click({force: true});
      await umbracoUi.dataType.clickSubmitButton();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test(`can update Content App Name`, async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const contentAppName = 'Test Content App Name';
      const expectedDataTypeValues = {
        alias: "tabName",
        value: contentAppName,
      };

      // Act
      await umbracoUi.dataType.enterContentAppName(contentAppName);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test(`can enable Show Content App First`, async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const expectedDataTypeValues = {
        alias: "showContentFirst",
        value: true,
      };

      // Act
      await umbracoUi.dataType.clickShowContentAppFirstSlider();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test(`can enable Edit in Infinite Editor`, async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const expectedDataTypeValues = {
        alias: "useInfiniteEditor",
        value: true,
      };

      // Act
      await umbracoUi.dataType.clickEditInInfiniteEditorSlider();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      //await umbracoUi.dataType.isSuccessNotificationVisible();
      dataTypeData = await umbracoApi.dataType.getByName(listViewType);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });
  });
}
