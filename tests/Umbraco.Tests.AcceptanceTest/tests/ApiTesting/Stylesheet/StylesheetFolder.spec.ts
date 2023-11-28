import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheet Folder tests', () => {
  let stylesheetFolderPath = "";
  const stylesheetFolderName = 'StylesheetFolder';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.deleteFolder(stylesheetFolderPath);
  });

  test('can create a stylesheet folder', async ({umbracoApi}) => {
    // Arrange
    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    // Assert
    await expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetFolderPath)).toBeTruthy();
  });

  test('can delete a stylesheet folder', async ({umbracoApi}) => {
    // Arrange
    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
    await expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetFolderPath)).toBeTruthy();

    // Act
    await umbracoApi.stylesheet.deleteFolder(stylesheetFolderPath);

    // Assert
    await expect(await umbracoApi.stylesheet.doesExist(stylesheetFolderPath)).toBeFalsy();
  });

  test('can add a stylesheet folder in another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    // Act
    await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderPath);

    // Assert
    const children = await umbracoApi.stylesheet.getChildren(stylesheetFolderPath);
    await expect(children[0].name).toEqual(childFolderName);
  });

  test('can add a stylesheet folder in a folder that is in another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Creates parent folder
    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    // Creates child folder in parent folder
    const childOfParentPath = await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderPath);
    const childOfParent = await umbracoApi.stylesheet.getChildren(stylesheetFolderPath);

    // Act
    // Creates childOfChild folder in child folder
    await umbracoApi.stylesheet.createFolder(childOfChildFolderName, childOfParentPath);

    // Assert
    // Checks if the stylesheet folder are in the correct folders
    await expect(childOfParent[0].name).toEqual(childFolderName);
    const childOfChild = await umbracoApi.stylesheet.getChildren(childOfParentPath);
    await expect(childOfChild[0].name).toEqual(childOfChildFolderName);
  });

  test('can delete a stylesheet folder from another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
    const childPath = await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderPath);
    const child = await umbracoApi.stylesheet.getChildren(stylesheetFolderPath);

    // Checks if a child exists in the parent folder with the name
    await expect(child[0].name).toEqual(childFolderName);
    await umbracoApi.stylesheet.doesFolderExist(childPath);

    // Act
    await umbracoApi.stylesheet.deleteFolder(childPath);

    // Assert
    const noChild = await umbracoApi.stylesheet.getChildren(stylesheetFolderPath);
    await expect(noChild[0]).toEqual(undefined);
  });
});
