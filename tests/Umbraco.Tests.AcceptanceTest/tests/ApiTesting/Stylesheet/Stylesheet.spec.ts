import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheet tests', () => {
  const stylesheetName = 'Stylesheet.css';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.stylesheet.ensureStylesheetNameNotExistsAtRoot(stylesheetName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.stylesheet.ensureStylesheetNameNotExistsAtRoot(stylesheetName);
  });

  test('can create a stylesheet', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.stylesheet.createStylesheet(stylesheetName, 'content');

    // Assert
    await expect(await umbracoApi.stylesheet.doesStylesheetWithNameExistAtRoot(stylesheetName)).toBeTruthy();
  });

  test('can update a stylesheet', async ({page, umbracoApi, umbracoUi}) => {
    const newContent = 'BetterContent';

    await umbracoApi.stylesheet.createStylesheet(stylesheetName, 'content');

    const stylesheet = await umbracoApi.stylesheet.getStylesheetByNameAtRoot(stylesheetName);

    stylesheet.content = newContent;

    await umbracoApi.stylesheet.updateStylesheet(stylesheet);

    // Assert
    // Checks if the content was updated for the stylesheet
    const updatedStylesheet = await umbracoApi.stylesheet.getStylesheetByPath(stylesheet.path);
    await expect(updatedStylesheet.content === newContent).toBeTruthy();
  });

  test('can delete a stylesheet', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.stylesheet.createStylesheet(stylesheetName, 'content');

    await expect(await umbracoApi.stylesheet.doesStylesheetWithNameExistAtRoot(stylesheetName)).toBeTruthy();

    const stylesheet = await umbracoApi.stylesheet.getStylesheetByNameAtRoot(stylesheetName);

    await umbracoApi.stylesheet.deleteStylesheetByPath(stylesheet.path);

    // Assert
    await expect(await umbracoApi.stylesheet.doesStylesheetWithNameExistAtRoot(stylesheetName)).toBeFalsy();
  });

  test('can create a stylesheet in a folder', async ({page, umbracoApi, umbracoUi}) => {
    const folderName = 'StyledFolder';

    await umbracoApi.stylesheet.ensureStylesheetNameNotExistsAtRoot(folderName);

    await umbracoApi.stylesheet.createStylesheetFolder(folderName);

    const folder = await umbracoApi.stylesheet.getStylesheetByNameAtRoot(folderName);

    await umbracoApi.stylesheet.createStylesheet(stylesheetName, 'content', folder.path);

    // Assert
    const child = await umbracoApi.stylesheet.getChildrenInStylesheetFolderByPath(folder.path);
    await expect(child.items[0].name === stylesheetName).toBeTruthy();

    // Clean
    await umbracoApi.stylesheet.ensureStylesheetNameNotExistsAtRoot(folderName);
  });

  test('can delete a stylesheet from a folder', async ({page, umbracoApi, umbracoUi}) => {
    const folderName = 'StyledFolder';

    await umbracoApi.stylesheet.ensureStylesheetNameNotExistsAtRoot(folderName);

    await umbracoApi.stylesheet.createStylesheetFolder(folderName);

    const folder = await umbracoApi.stylesheet.getStylesheetByNameAtRoot(folderName);

    await umbracoApi.stylesheet.createStylesheet(stylesheetName, 'deleteMe', folder.path);

    // Checks if the stylesheet was created
    const child = await umbracoApi.stylesheet.getChildrenInStylesheetFolderByPath(folder.path);

    await expect(child.items[0].name === stylesheetName).toBeTruthy();

    await umbracoApi.stylesheet.deleteStylesheetByPath(child.items[0].path);

    // Assert
    const noChild = await umbracoApi.stylesheet.getChildrenInStylesheetFolderByPath(folder.path);
    await expect(noChild.items[0] === undefined).toBeTruthy();

    // Clean
    await umbracoApi.stylesheet.ensureStylesheetNameNotExistsAtRoot(folderName);
  });
});
