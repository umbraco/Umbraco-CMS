import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('DataType tests', () => {
  let dataTypeId = "";

  const dataTypeName = "TestType";
  const propertyEditorAlias = "Umbraco.DateTime";
  const dataTypeData = [
    {
      "alias": "tester",
      "value": "Howdy"
    }
  ];

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExistsAtRoot(dataTypeName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.dataType.delete(dataTypeId);
  });

  test('can create dataType', async ({page, umbracoApi, umbracoUi}) => {
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, dataTypeData);

    // Assert
    await expect(umbracoApi.dataType.exists(dataTypeId)).toBeTruthy();
  });

  test('can update dataType', async ({page, umbracoApi, umbracoUi}) => {
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, []);

    const dataType = await umbracoApi.dataType.get(dataTypeId);

    dataType.values = dataTypeData;
    await umbracoApi.dataType.update(dataTypeId, dataType);

    // Assert
    // Checks if the data type was updated
    const updatedDataType = await umbracoApi.dataType.get(dataTypeId);
    await expect(updatedDataType.values).toEqual(dataTypeData);
  });

  test('can delete dataType', async ({page, umbracoApi, umbracoUi}) => {
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, dataTypeData);

    await expect(await umbracoApi.dataType.exists(dataTypeId)).toBeTruthy();

    await umbracoApi.dataType.delete(dataTypeId);

    // Assert
    await expect(await umbracoApi.dataType.exists(dataTypeId)).toBeFalsy();
  });

  test('can move a dataType to a folder', async ({page, umbracoApi, umbracoUi}) => {
    const folderName = 'FolderToMoveToo';

    await umbracoApi.dataType.ensureNameNotExistsAtRoot(folderName);

    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, dataTypeData);
    const dataTypeFolderId = await umbracoApi.dataType.createFolder(folderName);

    await umbracoApi.dataType.moveToFolder(dataTypeId, dataTypeFolderId);

    // Assert
    // Checks if the datatype has the parentId of the folder
    const dataTypeInFolder = await umbracoApi.dataType.getChildren(dataTypeFolderId);
    await expect(dataTypeInFolder.items[0].parentId).toEqual(dataTypeFolderId);

    // Clean
    await umbracoApi.dataType.deleteFolder(dataTypeFolderId);
  });

  test('can copy a dataType to a folder', async ({page, umbracoApi, umbracoUi}) => {
    const folderName = 'FolderToCopyToo';

    await umbracoApi.dataType.ensureNameNotExistsAtRoot(folderName);

    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, dataTypeData);
    const dataTypeFolderId = await umbracoApi.dataType.createFolder(folderName);

    const dataType = await umbracoApi.dataType.get(dataTypeId);
    const dataTypeFolder = await umbracoApi.dataType.getFolder(dataTypeFolderId);

    const copiedDataTypeId = await umbracoApi.dataType.copyToFolder(dataType.id, dataTypeFolder.id);

    const copiedDataType = await umbracoApi.dataType.get(copiedDataTypeId);

    await expect(copiedDataType.name).toEqual(dataTypeName + ' (copy)');

    // Assert
    // Checks if both dataTypes exists
    await expect(await umbracoApi.dataType.exists(dataTypeId)).toBeTruthy();
    await expect(await umbracoApi.dataType.exists(copiedDataTypeId)).toBeTruthy();

    // Clean
    await umbracoApi.dataType.deleteFolder(dataTypeFolderId);
  });
});
