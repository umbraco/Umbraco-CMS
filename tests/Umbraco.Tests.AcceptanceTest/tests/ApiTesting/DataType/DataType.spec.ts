import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('DataType tests', () => {
  const dataTypeName = "TestType";
  const propertyEditorAlias = "Umbraco.DateTime";
  const dataTypeData = [
    {
      "alias": "tester",
      "value": "Howdy"
    }
  ];

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.dataType.ensureDataTypeNameNotExistsAtRoot(dataTypeName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.dataType.ensureDataTypeNameNotExistsAtRoot(dataTypeName);
  });

  test('can create dataType', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dataType.createDataType(dataTypeName, propertyEditorAlias, dataTypeData);

    // Assert
    await expect(umbracoApi.dataType.doesDataTypeWithNameExistAtRoot(dataTypeName)).toBeTruthy();
  });

  test('can update dataType', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dataType.createDataType(dataTypeName, propertyEditorAlias, []);

    const dataType = await umbracoApi.dataType.getDataTypeByNameAtRoot(dataTypeName);

    dataType.values = dataTypeData;
    await umbracoApi.dataType.updateDataTypeById(dataType.id, dataTypeData);

    // Assert
    // Checks if the data type was updated
    const updatedDataType = await umbracoApi.dataType.getDataTypeByNameAtRoot(dataTypeName);
    await expect(updatedDataType.values = dataTypeData).toBeTruthy();
  });

  test('can delete dataType', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dataType.createDataType(dataTypeName, propertyEditorAlias, dataTypeData);

    await expect(await umbracoApi.dataType.doesDataTypeWithNameExistAtRoot(dataTypeName)).toBeTruthy();

    await umbracoApi.dataType.deleteDataTypeByNameAtRoot(dataTypeName);

    // Assert
    await expect(await umbracoApi.dataType.doesDataTypeWithNameExistAtRoot(dataTypeName)).toBeFalsy();
  });

  test('can move a dataType to a folder', async ({page, umbracoApi, umbracoUi}) => {
    const folderName = 'FolderToMoveToo';

    await umbracoApi.dataType.ensureDataTypeNameNotExistsAtRoot(folderName);

    await umbracoApi.dataType.createDataType(dataTypeName, propertyEditorAlias, dataTypeData);
    await umbracoApi.dataType.createDataTypeFolder(folderName);

    const dataType = await umbracoApi.dataType.getDataTypeByNameAtRoot(dataTypeName);
    const dataTypeFolder = await umbracoApi.dataType.getDataTypeFolderByName(folderName);

    await umbracoApi.dataType.moveDataTypeToFolderById(dataType.id, dataTypeFolder.id);

    // Assert
    const dataTypeInFolder = await umbracoApi.dataType.getDataTypeChildrenById(dataTypeFolder.id);
    await expect(dataTypeInFolder.items[0].name === dataTypeName).toBeTruthy();
    // Since the dataType was moved it should not be in the root anymore
    await expect(await umbracoApi.dataType.doesDataTypeWithNameExistAtRoot(dataTypeName)).toBeFalsy();

    // Clean
    await umbracoApi.dataType.ensureDataTypeNameNotExistsAtRoot(folderName);
  });

  test('can copy a dataType to a folder', async ({page, umbracoApi, umbracoUi}) => {
    const folderName = 'FolderToCopyToo';

    await umbracoApi.dataType.ensureDataTypeNameNotExistsAtRoot(folderName);

    await umbracoApi.dataType.createDataType(dataTypeName, propertyEditorAlias, dataTypeData);
    await umbracoApi.dataType.createDataTypeFolder(folderName);

    const dataType = await umbracoApi.dataType.getDataTypeByNameAtRoot(dataTypeName);
    const dataTypeFolder = await umbracoApi.dataType.getDataTypeFolderByName(folderName);

    await umbracoApi.dataType.copyDataTypeToFolderById(dataType.id, dataTypeFolder.id);

    const dataTypeInFolder = await umbracoApi.dataType.getDataTypeChildrenById(dataTypeFolder.id);

    // Assert
    await expect(dataTypeInFolder.items[0].name === dataTypeName + ' (copy)').toBeTruthy();
    await expect(await umbracoApi.dataType.doesDataTypeWithNameExistAtRoot(dataTypeName)).toBeTruthy();

    // Clean
    await umbracoApi.dataType.ensureDataTypeNameNotExistsAtRoot(folderName);
  });
});
