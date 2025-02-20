import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('DataType tests', () => {
  let dataTypeId = '';
  let dataTypeFolderId = '';
  let copiedDataTypeId = '';

  const dataTypeName = 'TestDataType';
  const folderName = 'TestDataTypeFolder';
  const editorAlias = 'Umbraco.DateTime';
  const editorUiAlias = 'Umb.PropertyEditorUi.DatePicker';
  const dataTypeData = [
    {
      "alias": "tester",
      "value": "Howdy"
    }
  ];

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
    if (dataTypeFolderId != '') {
      await umbracoApi.dataType.ensureNameNotExists(folderName);
    }
  });

  test('can create dataType', async ({umbracoApi}) => {
    // Act
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias, dataTypeData);

    // Assert
    expect(umbracoApi.dataType.doesExist(dataTypeId)).toBeTruthy();
  });

  test('can update dataType', async ({umbracoApi}) => {
    // Arrange
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias, []);
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
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias, dataTypeData);
    expect(await umbracoApi.dataType.doesExist(dataTypeId)).toBeTruthy();

    // Act
    await umbracoApi.dataType.delete(dataTypeId);

    // Assert
    expect(await umbracoApi.dataType.doesExist(dataTypeId)).toBeFalsy();
  });

  test('can move a dataType to a folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.dataType.ensureNameNotExists(folderName);
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias, dataTypeData);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
    dataTypeFolderId = await umbracoApi.dataType.createFolder(folderName);
    expect(await umbracoApi.dataType.doesFolderExist(dataTypeFolderId)).toBeTruthy();

    // Act
    await umbracoApi.dataType.moveToFolder(dataTypeId, dataTypeFolderId);

    // Assert
    // Checks if the datatype has the parentId of the folder
    const dataTypeInFolder = await umbracoApi.dataType.getChildren(dataTypeFolderId);
    expect(dataTypeInFolder[0].parent.id).toEqual(dataTypeFolderId);
  });

  test('can copy a dataType to a folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.dataType.ensureNameNotExists(folderName);
    dataTypeId = await umbracoApi.dataType.create(dataTypeName, editorAlias, editorUiAlias, dataTypeData);
    dataTypeFolderId = await umbracoApi.dataType.createFolder(folderName);

    const dataType = await umbracoApi.dataType.get(dataTypeId);
    const dataTypeFolder = await umbracoApi.dataType.getFolder(dataTypeFolderId);

    // Act
    copiedDataTypeId = await umbracoApi.dataType.copyToFolder(dataType.id, dataTypeFolder.id);

    // Assert
    const copiedDataType = await umbracoApi.dataType.get(copiedDataTypeId);
    expect(copiedDataType.name).toEqual(dataTypeName + ' (copy)');
    // Checks if both dataTypes exists
    expect(await umbracoApi.dataType.doesExist(dataTypeId)).toBeTruthy();
    expect(await umbracoApi.dataType.doesExist(copiedDataTypeId)).toBeTruthy();

    // Clean
    await umbracoApi.dataType.delete(copiedDataTypeId);
  });
});
