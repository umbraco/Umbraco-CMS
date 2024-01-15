import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheet tests', () => {
  const stylesheetName = 'Stylesheet.css';
  const folderName = 'StyledFolder';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
    await umbracoApi.stylesheet.ensureNameNotExists(folderName);
  });

  test('can create a stylesheet', async ({umbracoApi}) => {
    // Act
    await umbracoApi.stylesheet.create(stylesheetName, 'content');

    // Assert
    expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeTruthy();
  });

  test('can update a stylesheet', async ({umbracoApi}) => {
    // Arrange
    const newContent = 'BetterContent';
    await umbracoApi.stylesheet.create(stylesheetName, 'content');
    const stylesheet = await umbracoApi.stylesheet.get(stylesheetName);
    stylesheet.content = newContent;

    // Act
    await umbracoApi.stylesheet.update(stylesheet);

    // Assert
    // Checks if the content was updated for the stylesheet
    const updatedStylesheet = await umbracoApi.stylesheet.get(stylesheetName);
    expect(updatedStylesheet.content).toEqual(newContent);
  });

  test('can delete a stylesheet', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.stylesheet.create(stylesheetName, 'content');
    expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeTruthy();

    // Act
    await umbracoApi.stylesheet.delete(stylesheetName);

    // Assert
    expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeFalsy();
  });

  test('can create a stylesheet in a folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(folderName);
    await umbracoApi.stylesheet.createFolder(folderName);

    // Act
    await umbracoApi.stylesheet.create(stylesheetName, 'content', folderName);

    // Assert
    const child = await umbracoApi.stylesheet.getChildren(folderName);
    expect(child[0].name).toEqual(stylesheetName);
  });

  test('can delete a stylesheet from a folder', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(folderName);
    await umbracoApi.stylesheet.createFolder(folderName);
    await umbracoApi.stylesheet.create(stylesheetName, 'deleteMe', folderName);

    // Checks if the stylesheet was created
    const child = await umbracoApi.stylesheet.getChildren(folderName);
    // Checks if the child is in the folder
    expect(child[0].name).toEqual(stylesheetName);

    // Act
    await umbracoApi.stylesheet.delete(stylesheetName);

    // Assert
    const noChild = await umbracoApi.stylesheet.getChildren(folderName);
    expect(noChild[0].hasChildren).toEqual(false);
  });
});
