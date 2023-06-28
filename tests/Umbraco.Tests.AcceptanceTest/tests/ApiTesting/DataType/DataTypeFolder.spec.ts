import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('DataTypeFolder tests', () => {
  let dataTypeFolderId = "";
  const dataTypeFolderName = "TestTypeFolder";

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExistsAtRoot(dataTypeFolderName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.dataType.deleteFolder(dataTypeFolderId);
  });

  test('can create a dataType folder', async ({page, umbracoApi, umbracoUi}) => {
    dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);

    // Assert
    await expect(umbracoApi.dataType.folderExists(dataTypeFolderId)).toBeTruthy();
  });

  test('can update a dataType folder', async ({page, umbracoApi, umbracoUi}) => {
    const oldDataTypeFolderName = 'Oldie';

    dataTypeFolderId = await umbracoApi.dataType.createFolder(oldDataTypeFolderName);
    const dataTypeFolder = await umbracoApi.dataType.getFolder(dataTypeFolderId);

    // Updates the dataType folder
    dataTypeFolder.name = dataTypeFolderName;
    await umbracoApi.dataType.updateFolder(dataTypeFolderId, dataTypeFolder);

    // Assert
    await expect(umbracoApi.dataType.folderExists(dataTypeFolderId)).toBeTruthy();
    // Checks if the dataType folder was updated
    const newDataTypeFolderName = await umbracoApi.dataType.getFolder(dataTypeFolderId);
    await expect(newDataTypeFolderName.name).toEqual(dataTypeFolderName);
  });

  test('can delete a dataType folder', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dataType.createFolder(dataTypeFolderName);

    await expect(umbracoApi.dataType.folderExists(dataTypeFolderId)).toBeTruthy();

    await umbracoApi.dataType.delete(dataTypeFolderId);

    // Assert
    await expect(await umbracoApi.dataType.folderExists(dataTypeFolderId)).toBeFalsy();
  });
});
