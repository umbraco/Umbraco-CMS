import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View Folder tests', () => {
  const partialViewFolderName = 'partialViewFolder';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(partialViewFolderName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(partialViewFolderName);
  });

  test('can create a partial view folder', async ({umbracoApi}) => {
    // Act
    await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Assert
    expect(await umbracoApi.partialView.doesFolderExist(partialViewFolderName)).toBeTruthy();
  });

  test('can delete a partial view folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.partialView.createFolder(partialViewFolderName);
    expect(await umbracoApi.partialView.doesFolderExist(partialViewFolderName)).toBeTruthy();

    // Act
    await umbracoApi.partialView.deleteFolder(partialViewFolderName);

    // Assert
    expect(await umbracoApi.partialView.doesFolderExist(partialViewFolderName)).toBeFalsy();
  });

  test('can add a partial view folder in another', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Act
    await umbracoApi.partialView.createFolder(childFolderName, partialViewFolderName);
    const children = await umbracoApi.partialView.getChildren(partialViewFolderName);

    // Assert
    expect(children[0].name).toEqual(childFolderName);
  });

  test('can add a partial view folder in a folder in another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';
    const childFolderPath = partialViewFolderName + '/' + childFolderName;

    // Creates parent folder
    await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Creates child folder in parent folder
    await umbracoApi.partialView.createFolder(childFolderName, partialViewFolderName);

    // Act
    // Creates childOfChild folder in child folder
    await umbracoApi.partialView.createFolder(childOfChildFolderName, childFolderPath);

    // Assert
    // Checks if the partial views folder are in the correct folders
    const childOfParent = await umbracoApi.partialView.getChildren(partialViewFolderName);
    const childOfChild = await umbracoApi.partialView.getChildren(childFolderPath);
    expect(childOfParent[0].name).toEqual(childFolderName);
    expect(childOfChild[0].name).toEqual(childOfChildFolderName);
  });
});