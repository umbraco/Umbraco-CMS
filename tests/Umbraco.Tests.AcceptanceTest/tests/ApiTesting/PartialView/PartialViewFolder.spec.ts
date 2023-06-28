import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View Folder tests', () => {
  let partialViewFolderPath = "";
  const partialViewFolderName = 'partialViewFolder';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFolderName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.partialView.deleteFolder(partialViewFolderPath);
  });

  test('can create a partial view folder', async ({page, umbracoApi, umbracoUi}) => {
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Assert
    await expect(await umbracoApi.partialView.folderExists(partialViewFolderPath)).toBeTruthy();
  });

  test('can delete a partial view folder', async ({page, umbracoApi, umbracoUi}) => {
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    await expect(await umbracoApi.partialView.folderExists(partialViewFolderPath)).toBeTruthy();

    await umbracoApi.partialView.deleteFolder(partialViewFolderPath);

    // Assert
    await expect(await umbracoApi.partialView.folderExists(partialViewFolderPath)).toBeFalsy();
  });

  test('can add a partial view folder in another', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    await umbracoApi.partialView.getFolder(partialViewFolderPath);

    await umbracoApi.partialView.createFolder(childFolderName, partialViewFolderPath);

    const children = await umbracoApi.partialView.getFolderChildren(partialViewFolderPath);

    // Assert
    await expect(children.items[0].name).toEqual(childFolderName);
  });

  test('can add a partial view folder in a folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Creates parent folder
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Creates child folder in parent folder
    const childOfParentPath = await umbracoApi.partialView.createFolder(childFolderName, partialViewFolderPath);
    const childOfParent = await umbracoApi.partialView.getFolderChildren(partialViewFolderPath);

    // Creates childOfChild folder in child folder
    await umbracoApi.partialView.createFolder(childOfChildFolderName, childOfParent.childOfParentPath);
    const childOfChild = await umbracoApi.partialView.getFolderChildren(childOfParent.childOfParentPath);

    // Assert
    // Checks if the partial views folder are in the correct folders
    await expect(childOfParent.items[0].name).toEqual(childFolderName);
    await expect(childOfChild.items[0].name).toEqual(childOfChildFolderName);
  });
});
