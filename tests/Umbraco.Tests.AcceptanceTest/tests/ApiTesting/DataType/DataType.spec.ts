import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('DataType tests', () => {
  let dataTypeId = "";
  let dataTypeFolderId = "";

  const dataTypeName = "TestType";
  const propertyEditorAlias = "Umbraco.DateTime";
  const dataTypeData = [
    {
      "alias": "tester",
      "value": "Howdy"
    }
  ];

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExistsAtRoot(dataTypeName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.dataType.delete(dataTypeId);
  });

  test('can create dataType', async ({umbracoApi}) => {
    // Act
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, dataTypeData);

    // Assert
    expect(umbracoApi.dataType.exists(dataTypeId)).toBeTruthy();
  });

  test('can update dataType', async ({umbracoApi}) => {
    // Arrange
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, []);
    const dataType = await umbracoApi.dataType.get(dataTypeId);
    dataType.values = dataTypeData;

    // Act
    await umbracoApi.dataType.update(dataTypeId, dataType);

    // Assert
    // Checks if the data type was updated
    const updatedDataType = await umbracoApi.dataType.get(dataTypeId);
    expect(updatedDataType.values).toEqual(dataTypeData);
  });

  test('can delete dataType', async ({umbracoApi}) => {
    // Arrange
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, dataTypeData);
    expect(await umbracoApi.dataType.exists(dataTypeId)).toBeTruthy();

    // Act
    await umbracoApi.dataType.delete(dataTypeId);

    // Assert
    expect(await umbracoApi.dataType.exists(dataTypeId)).toBeFalsy();
  });

  test('can move a dataType to a folder', async ({umbracoApi}) => {
    // Arrange
    const folderName = 'FolderToMoveToo';
    await umbracoApi.dataType.ensureNameNotExistsAtRoot(folderName);
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, dataTypeData);
    dataTypeFolderId = await umbracoApi.dataType.createFolder(folderName);

    // Act
    await umbracoApi.dataType.moveToFolder(dataTypeId, dataTypeFolderId);

    // Assert
    // Checks if the datatype has the parentId of the folder
    const dataTypeInFolder = await umbracoApi.dataType.getChildren(dataTypeFolderId);
    expect(dataTypeInFolder.items[0].parentId).toEqual(dataTypeFolderId);

    // Clean
    await umbracoApi.dataType.deleteFolder(dataTypeFolderId);
  });

  test('can copy a dataType to a folder', async ({umbracoApi}) => {
    // Arrange
    let copiedDataTypeId = "";
    const folderName = 'FolderToCopyToo';
    await umbracoApi.dataType.ensureNameNotExistsAtRoot(folderName);

    dataTypeId = await umbracoApi.dataType.create(dataTypeName, propertyEditorAlias, dataTypeData);
    dataTypeFolderId = await umbracoApi.dataType.createFolder(folderName);

    const dataType = await umbracoApi.dataType.get(dataTypeId);
    const dataTypeFolder = await umbracoApi.dataType.getFolder(dataTypeFolderId);

    // Act
    copiedDataTypeId = await umbracoApi.dataType.copyToFolder(dataType.id, dataTypeFolder.id);

    // Assert
    const copiedDataType = await umbracoApi.dataType.get(copiedDataTypeId);
    expect(copiedDataType.name).toEqual(dataTypeName + ' (copy)');
    // Checks if both dataTypes exists
    expect(await umbracoApi.dataType.exists(dataTypeId)).toBeTruthy();
    expect(await umbracoApi.dataType.exists(copiedDataTypeId)).toBeTruthy();

    // Clean
    await umbracoApi.dataType.deleteFolder(dataTypeFolderId);
  });
});
