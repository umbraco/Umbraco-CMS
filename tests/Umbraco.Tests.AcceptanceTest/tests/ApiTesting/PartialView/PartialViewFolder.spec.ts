import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View Folder tests', () => {
  let partialViewFolderPath = '';
  const partialViewFolderName = 'partialViewFolder';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(partialViewFolderName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(partialViewFolderName);
  });

  test('can create a partial view folder', async ({umbracoApi}) => {
    // Act
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Assert
    expect(await umbracoApi.partialView.doesFolderExist(partialViewFolderPath)).toBeTruthy();
  });

  test('can delete a partial view folder', async ({umbracoApi}) => {
    // Arrange
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);
    expect(await umbracoApi.partialView.doesFolderExist(partialViewFolderPath)).toBeTruthy();

    // Act
    await umbracoApi.partialView.deleteFolder(partialViewFolderPath);

    // Assert
    expect(await umbracoApi.partialView.doesFolderExist(partialViewFolderPath)).toBeFalsy();
  });

  test('can add a partial view folder in another', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Act
    await umbracoApi.partialView.createFolder(childFolderName, partialViewFolderPath);

    // Assert
    const childrenData = await umbracoApi.partialView.getChildren(partialViewFolderPath);
    expect(childrenData[0].name).toEqual(childFolderName);
  });

  test('can add a partial view folder in a folder in another folder', async ({umbracoApi}) => {
    // Arrange
    const childFolderName = 'childFolder';
    const childOfChildFolderName = 'childOfChildFolder';

    // Creates parent folder
    partialViewFolderPath = await umbracoApi.partialView.createFolder(partialViewFolderName);

    // Creates child folder in parent folder
    const childFolderPath = await umbracoApi.partialView.createFolder(childFolderName, partialViewFolderPath);

    // Act
    // Creates childOfChild folder in child folder
    await umbracoApi.partialView.createFolder(childOfChildFolderName, childFolderPath);

    // Assert
    // Checks if the partial views folder are in the correct folders
    const childOfParentData = await umbracoApi.partialView.getChildren(partialViewFolderPath);
    expect(childOfParentData[0].name).toEqual(childFolderName);
    const childOfChildData = await umbracoApi.partialView.getChildren(childFolderPath);   
    expect(childOfChildData[0].name).toEqual(childOfChildFolderName);
  });
});