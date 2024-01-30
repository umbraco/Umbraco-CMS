import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Script Folder tests', () => {
  const scriptFolderName = 'ScriptFolder';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test('can create a script folder', async ({umbracoApi}) => {
    // Act
    await umbracoApi.script.createFolder(scriptFolderName);

    // Assert
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeTruthy();
  });

  test('can delete a script folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.script.createFolder(scriptFolderName);
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeTruthy();

    // Act
    await umbracoApi.script.deleteFolder(scriptFolderName);

    // Assert
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeFalsy();
  });

  test('can add a script folder in another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    await umbracoApi.script.createFolder(scriptFolderName);
    await umbracoApi.script.createFolder(childFolderName, scriptFolderName);

    // Act
    const childFolder = await umbracoApi.script.getChildren(scriptFolderName);

    // Assert
    expect(childFolder[0].name).toEqual(childFolderName);
  });

  test('can add a script folder in a folder in another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';
    const childFolderPath = scriptFolderName + '/' + childFolderName;

    // Creates parent folder
    await umbracoApi.script.createFolder(scriptFolderName);
    // Creates child folder in parent folder
    await umbracoApi.script.createFolder(childFolderName, scriptFolderName);
    
    // Act
    // Creates childOfChild folder in child folder
    await umbracoApi.script.createFolder(childOfChildFolderName, childFolderPath);
    
    // Assert
    // Checks if the script folder are in the correct folders
    const childOfParent = await umbracoApi.script.getChildren(scriptFolderName);
    const childOfChild = await umbracoApi.script.getChildren(childFolderPath);
    expect(childOfParent[0].name).toEqual(childFolderName);
    expect(childOfChild[0].name).toEqual(childOfChildFolderName);
  });
});
