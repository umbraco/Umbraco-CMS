import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheet Folder tests', () => {
  const stylesheetFolderName = 'StylesheetFolder';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
  });

  test('can create a stylesheet folder', async ({umbracoApi}) => {
    // Act
    await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    // Assert
    expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetFolderName)).toBeTruthy();
  });

  test('can delete a stylesheet folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
    expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetFolderName)).toBeTruthy();

    // Act
    await umbracoApi.stylesheet.deleteFolder(stylesheetFolderName);

    // Assert
    expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetFolderName)).toBeFalsy();
  });

  test('can add a stylesheet folder in another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    // Act
    await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderName);

    // Assert
    const children = await umbracoApi.stylesheet.getChildren(stylesheetFolderName);
    expect(children[0].name).toEqual(childFolderName);
  });

  test('can add a stylesheet folder in a folder in another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';
    const childFolderPath = stylesheetFolderName + '/' + childFolderName;
    // Creates parent folder
    await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
    // Creates child folder in parent folder
    await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderName);

    // Act
    // Creates childOfChild folder in child folder
    await umbracoApi.stylesheet.createFolder(childOfChildFolderName, childFolderPath);

    // Assert
    // Checks if the stylesheet folder are in the correct folders
    const childOfParent = await umbracoApi.stylesheet.getChildren(stylesheetFolderName);
    const childOfChild = await umbracoApi.stylesheet.getChildren(childFolderPath);
    expect(childOfParent[0].name).toEqual(childFolderName);
    expect(childOfChild[0].name).toEqual(childOfChildFolderName);
  });

  test('can delete a stylesheet folder from another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    const stylesheetParentFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
    const stylesheetChildFolderPath = await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderName);
    // Checks if a child exists in the parent folder with the name
    const child = await umbracoApi.stylesheet.getChildren(stylesheetParentFolderPath);
    expect(child[0].name).toEqual(childFolderName);
    expect(child[0].isFolder).toBe(true);
    expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetChildFolderPath)).toBeTruthy();

    // Act
    await umbracoApi.stylesheet.deleteFolder(stylesheetChildFolderPath);

    // Assert
    const noChild = await umbracoApi.stylesheet.getChildren(stylesheetParentFolderPath);
    expect(noChild[0]).toBe(undefined);
  });
});
