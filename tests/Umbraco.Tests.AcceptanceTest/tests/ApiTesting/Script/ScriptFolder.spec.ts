import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Script Folder tests', () => {
  const scriptFolderName = 'scriptFolder';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.ensureScriptNotNameNotExistsAtRoot(scriptFolderName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.ensureScriptNotNameNotExistsAtRoot(scriptFolderName);
  });

  test('can create script folder', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.script.createScriptFolder(scriptFolderName);

    // Assert
    await expect(umbracoApi.script.doesScriptWithNameExistAtRoot(scriptFolderName)).toBeTruthy();
  });

  test('can delete script folder', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.script.createScriptFolder(scriptFolderName);

    await expect(await umbracoApi.script.doesScriptWithNameExistAtRoot(scriptFolderName)).toBeTruthy();
    const scriptFolder = await umbracoApi.script.getScriptByNameAtRoot(scriptFolderName);

    await umbracoApi.script.deleteScriptFolder(scriptFolder.path);

    // Assert
    await expect(await umbracoApi.script.doesScriptWithNameExistAtRoot(scriptFolderName)).toBeFalsy();
  });

  test('can add a script folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    await umbracoApi.script.createScriptFolder(scriptFolderName);

    const parentFolder = await umbracoApi.script.getScriptByNameAtRoot(scriptFolderName);

    await umbracoApi.script.createScriptFolder(childFolderName, parentFolder.path);

    const children = await umbracoApi.script.getChildrenInScriptFolderByPath(parentFolder.path);

    // Assert
    await expect(children.items[0].name === childFolderName).toBeTruthy();
  });

  test('can add a script folder in a folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Creates parent folder
    await umbracoApi.script.createScriptFolder(scriptFolderName);
    const parentFolder = await umbracoApi.script.getScriptByNameAtRoot(scriptFolderName);

    // Creates child folder in parent folder
    await umbracoApi.script.createScriptFolder(childFolderName, parentFolder.path);
    const childOfParent = await umbracoApi.script.getChildrenInScriptFolderByPath(parentFolder.path);

    // Creates childOfChild folder in child folder
    await umbracoApi.script.createScriptFolder(childOfChildFolderName, childOfParent.items[0].path);
    const childOfChild = await umbracoApi.script.getChildrenInScriptFolderByPath(childOfParent.items[0].path);

    // Assert
    // Checks if the script folder are in the correct folders
    await expect(childOfParent.items[0].name === childFolderName).toBeTruthy();
    await expect(childOfChild.items[0].name === childOfChildFolderName).toBeTruthy();
  });
});
