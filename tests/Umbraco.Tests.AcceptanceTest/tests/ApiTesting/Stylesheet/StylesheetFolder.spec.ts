import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheet Folder tests', () => {
  const stylesheetFolderName = 'StylesheetFolder';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.stylesheet.ensureStylesheetNameNotExistsAtRoot(stylesheetFolderName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.stylesheet.ensureStylesheetNameNotExistsAtRoot(stylesheetFolderName);
  });

  test('can create stylesheet folder', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.stylesheet.createStylesheetFolder(stylesheetFolderName);

    // Assert
    await expect(await umbracoApi.stylesheet.doesStylesheetWithNameExistAtRoot(stylesheetFolderName)).toBeTruthy();
  });

  test('can delete stylesheet folder', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.stylesheet.createStylesheetFolder(stylesheetFolderName);

    await expect(await umbracoApi.stylesheet.doesStylesheetWithNameExistAtRoot(stylesheetFolderName)).toBeTruthy();

    const stylesheet = await umbracoApi.stylesheet.getStylesheetByNameAtRoot(stylesheetFolderName);

    await umbracoApi.stylesheet.deleteStylesheetFolder(stylesheet.path);

    // Assert
    await expect(await umbracoApi.stylesheet.doesStylesheetWithNameExistAtRoot(stylesheetFolderName)).toBeFalsy();
  });

  test('can add a stylesheet folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    await umbracoApi.stylesheet.createStylesheetFolder(stylesheetFolderName);

    const parentFolder = await umbracoApi.stylesheet.getStylesheetByNameAtRoot(stylesheetFolderName);

    await umbracoApi.stylesheet.createStylesheetFolder(childFolderName, parentFolder.path);

    const children = await umbracoApi.stylesheet.getChildrenInStylesheetFolderByPath(parentFolder.path);

    // Assert
    await expect(children.items[0].name === childFolderName).toBeTruthy();
  });

  test('can add a stylesheet folder in a folder that is in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Creates parent folder
    await umbracoApi.stylesheet.createStylesheetFolder(stylesheetFolderName);

    const parentFolder = await umbracoApi.stylesheet.getStylesheetByNameAtRoot(stylesheetFolderName);

    // Creates child folder in parent folder
    await umbracoApi.stylesheet.createStylesheetFolder(childFolderName, parentFolder.path);

    const childOfParent = await umbracoApi.stylesheet.getChildrenInStylesheetFolderByPath(parentFolder.path);

    // Creates childOfChild folder in child folder
    await umbracoApi.stylesheet.createStylesheetFolder(childOfChildFolderName, childOfParent.items[0].path);

    const childOfChild = await umbracoApi.stylesheet.getChildrenInStylesheetFolderByPath(childOfParent.items[0].path);

    // Assert
    // Checks if the stylesheet folder are in the correct folders
    await expect(childOfParent.items[0].name === childFolderName).toBeTruthy();
    await expect(childOfChild.items[0].name === childOfChildFolderName).toBeTruthy();
  });

  test('can delete a stylesheet folder from another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';

    await umbracoApi.stylesheet.createStylesheetFolder(stylesheetFolderName);

    const parentFolder = await umbracoApi.stylesheet.getStylesheetByNameAtRoot(stylesheetFolderName);

    await umbracoApi.stylesheet.createStylesheetFolder(childFolderName, parentFolder.path);

    const child = await umbracoApi.stylesheet.getChildrenInStylesheetFolderByPath(parentFolder.path);

    await expect(child.items[0].name === childFolderName).toBeTruthy();

    await umbracoApi.stylesheet.doesStylesheetWithPathExist(child.items[0].path);

    await umbracoApi.stylesheet.deleteStylesheetFolder(child.items[0].path);

    const noChild = await umbracoApi.stylesheet.getChildrenInStylesheetFolderByPath(parentFolder.path);

    // Assert
    await expect(noChild.items[0] === undefined).toBeTruthy();
  });
});
