import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheet tests', () => {
  const stylesheetName = 'Stylesheet.css';
  const folderName = 'StyledFolder';
  let stylesheetPath = '';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
    await umbracoApi.stylesheet.ensureNameNotExists(folderName);
  });

  test('can create a stylesheet', async ({umbracoApi}) => {
    // Act
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content');

    // Assert
    expect(await umbracoApi.stylesheet.doesExist(stylesheetPath)).toBeTruthy();
  });

  test('can update stylesheet name', async ({umbracoApi}) => {
    // Arrange
    const oldStylesheetName = 'RandomStylesheetName.css';
    await umbracoApi.script.ensureNameNotExists(oldStylesheetName);
    const oldScriptPath = await umbracoApi.stylesheet.create(oldStylesheetName, 'content');

    // Act
    stylesheetPath = await umbracoApi.stylesheet.updateName(oldScriptPath, stylesheetName);

    // Assert
    // Checks if the name was updated for the stylesheet
    const updatedStylesheet = await umbracoApi.stylesheet.get(stylesheetPath);
    expect(updatedStylesheet.name).toEqual(stylesheetName);
  });

  test('can update stylesheet content', async ({umbracoApi}) => {
    // Arrange
    const newContent = 'BetterContent';
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content');

    // Act
    await umbracoApi.stylesheet.updateContent(stylesheetPath, newContent);

    // Assert
    // Checks if the content was updated for the stylesheet
    const updatedStylesheet = await umbracoApi.stylesheet.get(stylesheetPath);
    expect(updatedStylesheet.content).toEqual(newContent);
  });

  test('can delete a stylesheet', async ({umbracoApi}) => {
    // Arrange
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content');
    expect(await umbracoApi.stylesheet.doesExist(stylesheetPath)).toBeTruthy();

    // Act
    await umbracoApi.stylesheet.delete(stylesheetName);

    // Assert
    expect(await umbracoApi.stylesheet.doesExist(stylesheetPath)).toBeFalsy();
  });

  test('can create a stylesheet in a folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(folderName);
    const stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(folderName);

    // Act
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'content', folderName);

    // Assert
    const child = await umbracoApi.stylesheet.getChildren(stylesheetFolderPath);
    expect(child[0].name).toEqual(stylesheetName);
  });

  test('can delete a stylesheet from a folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(folderName);
    const stylesheetFolderPath = await umbracoApi.stylesheet.createFolder(folderName);
    stylesheetPath = await umbracoApi.stylesheet.create(stylesheetName, 'deleteMe', folderName);
    // Checks if the stylesheet was created
    const child = await umbracoApi.stylesheet.getChildren(stylesheetFolderPath);
    // Checks if the child is in the folder
    expect(child[0].name).toEqual(stylesheetName);

    // Act
    await umbracoApi.stylesheet.delete(stylesheetPath);

    // Assert
    const noChild = await umbracoApi.stylesheet.getChildren(stylesheetFolderPath);
    // Checks if the children is empty
    expect(noChild[0]).toEqual(undefined);
  });
});
