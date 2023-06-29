import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheet Folder tests', () => {
  let stylesheetFolderPath = "";
  const stylesheetFolderName = 'StylesheetFolder';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExistsAtRoot(stylesheetFolderName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.stylesheet.deleteFolder(stylesheetFolderPath);
  });

  test('can create a stylesheet folder', async ({page, umbracoApi, umbracoUi}) => {
    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    // Assert
    await expect(await umbracoApi.stylesheet.folderExists(stylesheetFolderPath)).toBeTruthy();
  });

  test('can delete a stylesheet folder', async ({page, umbracoApi, umbracoUi}) => {
    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    await expect(await umbracoApi.stylesheet.folderExists(stylesheetFolderPath)).toBeTruthy();

    await umbracoApi.stylesheet.deleteFolder(stylesheetFolderPath);

    // Assert
    await expect(await umbracoApi.stylesheet.exists(stylesheetFolderPath)).toBeFalsy();
  });

  test('can add a stylesheet folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderPath);

    const children = await umbracoApi.stylesheet.getFolderChildren(stylesheetFolderPath);

    // Assert
    await expect(children.items[0].name).toEqual(childFolderName);
  });

  test('can add a stylesheet folder in a folder that is in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Creates parent folder
    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    // Creates child folder in parent folder
    const childOfParentPath = await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderPath);

    const childOfParent = await umbracoApi.stylesheet.getFolderChildren(stylesheetFolderPath);

    // Creates childOfChild folder in child folder
    await umbracoApi.stylesheet.createFolder(childOfChildFolderName, childOfParentPath);

    const childOfChild = await umbracoApi.stylesheet.getFolderChildren(childOfParentPath);

    // Assert
    // Checks if the stylesheet folder are in the correct folders
    await expect(childOfParent.items[0].name).toEqual(childFolderName);
    await expect(childOfChild.items[0].name).toEqual(childOfChildFolderName);
  });

  test('can delete a stylesheet folder from another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';

    stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(stylesheetFolderName);

    const childPath = await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderPath);

    const child = await umbracoApi.stylesheet.getFolderChildren(stylesheetFolderPath);

    // Checks if a child exists in the parent folder with the name
    await expect(child.items[0].name).toEqual(childFolderName);

    await umbracoApi.stylesheet.folderExists(childPath);

    await umbracoApi.stylesheet.deleteFolder(childPath);

    const noChild = await umbracoApi.stylesheet.getFolderChildren(stylesheetFolderPath);

    // Assert
    await expect(noChild.items[0]).toEqual(undefined);
  });
});
