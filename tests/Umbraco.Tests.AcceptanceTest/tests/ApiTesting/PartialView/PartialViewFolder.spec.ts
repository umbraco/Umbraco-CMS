import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View Folder tests', () => {
  const partialViewFolderName = 'partialViewFolder';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.partialView.ensurePartialViewNameNotExistsAtRoot(partialViewFolderName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.partialView.ensurePartialViewNameNotExistsAtRoot(partialViewFolderName);
  });

  test('can create partial view folder', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.partialView.createPartialViewFolder(partialViewFolderName);

    // Assert
    await expect(umbracoApi.partialView.doesPartialViewWithNameExistAtRoot(partialViewFolderName)).toBeTruthy();
  });

  test('can delete partial view folder', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.partialView.createPartialViewFolder(partialViewFolderName);

    await expect(await umbracoApi.partialView.doesPartialViewWithNameExistAtRoot(partialViewFolderName)).toBeTruthy();
    const partialViewFolder = await umbracoApi.partialView.getPartialViewByNameAtRoot(partialViewFolderName);

    await umbracoApi.partialView.deletePartialViewFolder(partialViewFolder.path);

    // Assert
    await expect(await umbracoApi.partialView.doesPartialViewWithNameExistAtRoot(partialViewFolderName)).toBeFalsy();
  });

  test('can add a partial view folder in another', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    await umbracoApi.partialView.createPartialViewFolder(partialViewFolderName);

    const parentFolder = await umbracoApi.partialView.getPartialViewByNameAtRoot(partialViewFolderName);

    await umbracoApi.partialView.createPartialViewFolder(childFolderName, parentFolder.path);

    const children = await umbracoApi.partialView.getChildrenInPartialViewFolderByPath(parentFolder.path);

    // Assert
    await expect(children.items[0].name === childFolderName).toBeTruthy();
  });

  test('can add a partial view folder in a folder in another folder', async ({page, umbracoApi, umbracoUi}) => {
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Creates parent folder
    await umbracoApi.partialView.createPartialViewFolder(partialViewFolderName);
    const parentFolder = await umbracoApi.partialView.getPartialViewByNameAtRoot(partialViewFolderName);

    // Creates child folder in parent folder
    await umbracoApi.partialView.createPartialViewFolder(childFolderName, parentFolder.path);
    const childOfParent = await umbracoApi.partialView.getChildrenInPartialViewFolderByPath(parentFolder.path);

    // Creates childOfChild folder in child folder
    await umbracoApi.partialView.createPartialViewFolder(childOfChildFolderName, childOfParent.items[0].path);
    const childOfChild = await umbracoApi.partialView.getChildrenInPartialViewFolderByPath(childOfParent.items[0].path);

    // Assert
    // Checks if the partial views folder are in the correct folders
    await expect(childOfParent.items[0].name === childFolderName).toBeTruthy();
    await expect(childOfChild.items[0].name === childOfChildFolderName).toBeTruthy();
  });
});
