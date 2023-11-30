import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View Folder tests', () => {
  let partialViewFolderPath = "";
  const partialViewFolderName = 'partialViewFolder';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(partialViewFolderName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.deleteFolder(partialViewFolderPath);
  });

  test('can create a partial view folder', async ({umbracoApi}) => {
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Assert
    await expect(await umbracoApi.partialView.doesFolderExist(partialViewFolderPath)).toBeTruthy();
  });

  test('can delete a partial view folder', async ({umbracoApi}) => {
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    await expect(await umbracoApi.partialView.doesFolderExist(partialViewFolderPath)).toBeTruthy();

    await umbracoApi.partialView.deleteFolder(partialViewFolderPath);

    // Assert
    await expect(await umbracoApi.partialView.doesFolderExist(partialViewFolderPath)).toBeFalsy();
  });

  test('can add a partial view folder in another', async ({umbracoApi}) => {
    const childFolderName = 'childFolder';
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    await umbracoApi.partialView.getFolder(partialViewFolderPath);

    await umbracoApi.partialView.createFolder(childFolderName, partialViewFolderPath);

    const children = await umbracoApi.partialView.getFolderChildren(partialViewFolderPath);

    // Assert
    await expect(children.items[0].name).toEqual(childFolderName);
  });

  test('can add a partial view folder in a folder in another folder', async ({umbracoApi}) => {
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Creates parent folder
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Creates child folder in parent folder
    const childOfParentPath = await umbracoApi.partialView.createFolder(childFolderName, partialViewFolderPath);
    const childOfParent = await umbracoApi.partialView.getFolderChildren(partialViewFolderPath);

    // Creates childOfChild folder in child folder
    await umbracoApi.partialView.createFolder(childOfChildFolderName, childOfParentPath);
    const childOfChild = await umbracoApi.partialView.getFolderChildren(childOfParentPath);

    // Assert
    // Checks if the partial views folder are in the correct folders
    await expect(childOfParent.items[0].name).toEqual(childFolderName);
    await expect(childOfChild.items[0].name).toEqual(childOfChildFolderName);
  });
});