import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('DataTypeFolder tests', () => {
  let dataTypeFolderId = "";
  const dataTypeFolderName = "TestTypeFolder";

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExistsAtRoot(dataTypeFolderName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.dataType.deleteFolder(dataTypeFolderId);
  });

  test('can create a dataType folder', async ({umbracoApi}) => {
    // Act
    dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);

    // Assert
    expect(umbracoApi.dataType.folderExists(dataTypeFolderId)).toBeTruthy();
  });

  test('can update a dataType folder', async ({umbracoApi}) => {
    // Arrange
    const oldDataTypeFolderName = 'Oldie';

    dataTypeFolderId = await umbracoApi.dataType.createFolder(oldDataTypeFolderName);
    const dataTypeFolder = await umbracoApi.dataType.getFolder(dataTypeFolderId);
    dataTypeFolder.name = dataTypeFolderName;

    // Act
    await umbracoApi.dataType.updateFolder(dataTypeFolderId, dataTypeFolder);

    // Assert
    expect(umbracoApi.dataType.folderExists(dataTypeFolderId)).toBeTruthy();
    // Checks if the dataType folder was updated
    const newDataTypeFolderName = await umbracoApi.dataType.getFolder(dataTypeFolderId);
    expect(newDataTypeFolderName.name).toEqual(dataTypeFolderName);
  });

  test('can delete a dataType folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.dataType.createFolder(dataTypeFolderName);
    expect(umbracoApi.dataType.folderExists(dataTypeFolderId)).toBeTruthy();

    // Act
    await umbracoApi.dataType.delete(dataTypeFolderId);

    // Assert
    expect(await umbracoApi.dataType.folderExists(dataTypeFolderId)).toBeFalsy();
  });
});
