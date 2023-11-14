import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Script Folder tests', () => {
  let scriptFolderPath = '';
  const scriptFolderName = 'ScriptFolder';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test('can create a script folder', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    scriptFolderPath = await umbracoApi.script.createFolder(scriptFolderName);

    // Assert
    expect(await umbracoApi.script.doesFolderExist(scriptFolderPath)).toBeTruthy();

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test('can delete a script folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    scriptFolderPath = await umbracoApi.script.createFolder(scriptFolderName);
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeTruthy();

    // Act
    await umbracoApi.script.deleteFolder(scriptFolderPath);

    // Assert
    expect(await umbracoApi.script.doesFolderExist(scriptFolderPath)).toBeFalsy();
  });

  test('can add a script folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    scriptFolderPath = await umbracoApi.script.createFolder(scriptFolderName);
    await umbracoApi.script.createFolder(childFolderName, scriptFolderPath);

    // Act
    const childFolder = await umbracoApi.script.getChildren(scriptFolderPath);

    // Assert
    expect(childFolder[0].name).toEqual(childFolderName);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test('can add a script folder in a folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Act
    // Creates parent folder
    scriptFolderPath = await umbracoApi.script.createFolder(scriptFolderName);
    // Creates child folder in parent folder
    const childOfParentPath = await umbracoApi.script.createFolder(childFolderName, scriptFolderPath);
    const childOfParent = await umbracoApi.script.getChildren(scriptFolderPath);
    // Creates childOfChild folder in child folder
    await umbracoApi.script.createFolder(childOfChildFolderName, childOfParentPath);
    const childOfChild = await umbracoApi.script.getChildren(childOfParentPath);

    // Assert
    // Checks if the script folder are in the correct folders
    expect(childOfParent[0].name).toEqual(childFolderName);
    expect(childOfChild[0].name).toEqual(childOfChildFolderName);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });
});
