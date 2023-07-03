import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheet tests', () => {
  let stylesheetPath = "";
  const stylesheetName = 'Stylesheet.css';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExistsAtRoot(stylesheetName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.stylesheet.delete(stylesheetPath);
  });

  test('can create a stylesheet', async ({page, umbracoApi, umbracoUi}) => {
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content');

    // Assert
    await expect(await umbracoApi.stylesheet.exists(stylesheetPath)).toBeTruthy();
  });

  test('can update a stylesheet', async ({page, umbracoApi, umbracoUi}) => {
    const newContent = 'BetterContent';

    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content');

    const stylesheet = await umbracoApi.stylesheet.get(stylesheetPath);

    stylesheet.content = newContent;

    await umbracoApi.stylesheet.update(stylesheet);

    // Assert
    // Checks if the content was updated for the stylesheet
    const updatedStylesheet = await umbracoApi.stylesheet.get(stylesheetPath);
    await expect(updatedStylesheet.content).toEqual(newContent);
  });

  test('can delete a stylesheet', async ({page, umbracoApi, umbracoUi}) => {
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content');

    await expect(await umbracoApi.stylesheet.exists(stylesheetPath)).toBeTruthy();

    await umbracoApi.stylesheet.delete(stylesheetPath);

    // Assert
    await expect(await umbracoApi.stylesheet.exists(stylesheetPath)).toBeFalsy();
  });

  test('can create a stylesheet in a folder', async ({page, umbracoApi, umbracoUi}) => {
    const folderName = 'StyledFolder';

    await umbracoApi.stylesheet.ensureNameNotExistsAtRoot(folderName);

    stylesheetPath = await umbracoApi.stylesheet.createFolder(folderName);

    await umbracoApi.stylesheet.create(stylesheetName, 'content', stylesheetPath);

    // Assert
    const child = await umbracoApi.stylesheet.getFolderChildren(stylesheetPath);
    await expect(child.items[0].name).toEqual(stylesheetName);

    // Clean
    await umbracoApi.stylesheet.deleteFolder(stylesheetPath);
  });

  test('can delete a stylesheet from a folder', async ({page, umbracoApi, umbracoUi}) => {
    const folderName = 'StyledFolder';

    await umbracoApi.stylesheet.ensureNameNotExistsAtRoot(folderName);

    stylesheetPath = await umbracoApi.stylesheet.createFolder(folderName);

    const stylesheetChildPath = await umbracoApi.stylesheet.create(stylesheetName, 'deleteMe', stylesheetPath);

    // Checks if the stylesheet was created
    const child = await umbracoApi.stylesheet.getFolderChildren(stylesheetPath);

    // Checks if the child is in the folder
    await expect(child.items[0].name).toEqual(stylesheetName);

    await umbracoApi.stylesheet.delete(stylesheetChildPath);

    // Assert
    const noChild = await umbracoApi.stylesheet.getFolderChildren(stylesheetPath);
    await expect(noChild.items[0]).toEqual(undefined);

    // Clean
    await umbracoApi.stylesheet.deleteFolder(stylesheetPath);
  });
});
