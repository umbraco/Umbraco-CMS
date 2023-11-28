import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheet tests', () => {
  let stylesheetPath = "";
  const stylesheetName = 'Stylesheet.css';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.delete(stylesheetPath);
  });

  test('can create a stylesheet', async ({umbracoApi}) => {
    // Act
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content');

    // Assert
    await expect(await umbracoApi.stylesheet.doesExist(stylesheetPath)).toBeTruthy();
  });

  test('can update a stylesheet', async ({umbracoApi}) => {
    // Arrange
    const newContent = 'BetterContent';
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content');
    const stylesheet = await umbracoApi.stylesheet.get(stylesheetPath);
    stylesheet.content = newContent;

    // Act
    await umbracoApi.stylesheet.update(stylesheet);

    // Assert
    // Checks if the content was updated for the stylesheet
    const updatedStylesheet = await umbracoApi.stylesheet.get(stylesheetPath);
    await expect(updatedStylesheet.content).toEqual(newContent);
  });

  test('can delete a stylesheet', async ({umbracoApi}) => {
    // Arrange
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content');
    await expect(await umbracoApi.stylesheet.doesExist(stylesheetPath)).toBeTruthy();

    // Act
    await umbracoApi.stylesheet.delete(stylesheetPath);

    // Assert
    await expect(await umbracoApi.stylesheet.doesExist(stylesheetPath)).toBeFalsy();
  });

  test('can create a stylesheet in a folder', async ({umbracoApi}) => {
    // Arrange
    const folderName = 'StyledFolder';
    await umbracoApi.stylesheet.ensureNameNotExists(folderName);
    stylesheetPath = await umbracoApi.stylesheet.createFolder(folderName);

    // Act
    await umbracoApi.stylesheet.create(stylesheetName, 'content', stylesheetPath);

    // Assert
    const child = await umbracoApi.stylesheet.getChildren(stylesheetPath);
    await expect(child[0].name).toEqual(stylesheetName);

    // Clean
    await umbracoApi.stylesheet.deleteFolder(stylesheetPath);
  });

  test('can delete a stylesheet from a folder', async ({umbracoApi}) => {
    // Arrange
    const folderName = 'StyledFolder';
    await umbracoApi.stylesheet.ensureNameNotExists(folderName);
    stylesheetPath = await umbracoApi.stylesheet.createFolder(folderName);
    const stylesheetChildPath = await umbracoApi.stylesheet.create(stylesheetName, 'deleteMe', stylesheetPath);

    // Checks if the stylesheet was created
    const child = await umbracoApi.stylesheet.getChildren(stylesheetPath);
    // Checks if the child is in the folder
    await expect(child[0].name).toEqual(stylesheetName);

    // Act
    await umbracoApi.stylesheet.delete(stylesheetChildPath);

    // Assert
    const noChild = await umbracoApi.stylesheet.getChildren(stylesheetPath);
    await expect(noChild[0]).toEqual(undefined);

    // Clean
    await umbracoApi.stylesheet.deleteFolder(stylesheetPath);
  });
});
