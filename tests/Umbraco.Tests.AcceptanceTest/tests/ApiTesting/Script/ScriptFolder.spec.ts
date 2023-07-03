import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Script Folder tests', () => {
  let scriptFolderPath = "";
  const scriptFolderName = 'scriptFolder';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExistsAtRoot(scriptFolderName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.deleteFolder(scriptFolderPath);
  });

  test('can create a script folder', async ({page, umbracoApi, umbracoUi}) => {
    scriptFolderPath = await umbracoApi.script.createFolder(scriptFolderName);

    // Assert
    await expect(umbracoApi.script.folderExists(scriptFolderPath)).toBeTruthy();
  });

  test('can delete a script folder', async ({page, umbracoApi, umbracoUi}) => {
    scriptFolderPath = await umbracoApi.script.createFolder(scriptFolderName);

    await expect(await umbracoApi.script.folderExists(scriptFolderName)).toBeTruthy();

    await umbracoApi.script.deleteFolder(scriptFolderPath);

    // Assert
    await expect(await umbracoApi.script.folderExists(scriptFolderPath)).toBeFalsy();
  });

  test('can add a script folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    scriptFolderPath = await umbracoApi.script.createFolder(scriptFolderName);

    await umbracoApi.script.createFolder(childFolderName, scriptFolderPath);

    const childFolder = await umbracoApi.script.getFolderChildren(scriptFolderPath);

    // Assert
    await expect(childFolder.items[0].name).toEqual(childFolderName);
  });

  test('can add a script folder in a folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Creates parent folder
    scriptFolderPath = await umbracoApi.script.createFolder(scriptFolderName);

    // Creates child folder in parent folder
    const childOfParentPath = await umbracoApi.script.createFolder(childFolderName, scriptFolderPath);
    const childOfParent = await umbracoApi.script.getFolderChildren(scriptFolderPath);

    // Creates childOfChild folder in child folder
    await umbracoApi.script.createFolder(childOfChildFolderName, childOfParentPath);
    const childOfChild = await umbracoApi.script.getFolderChildren(childOfParentPath);

    // Assert
    // Checks if the script folder are in the correct folders
    await expect(childOfParent.items[0].name).toEqual(childFolderName);
    await expect(childOfChild.items[0].name).toEqual(childOfChildFolderName);
  });
});
