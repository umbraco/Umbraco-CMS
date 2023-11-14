import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Script tests', () => {
  let scriptPath = "";
  const scriptName = 'scriptName.js';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test('can create a script', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    scriptPath = await umbracoApi.script.create(scriptName, 'test');

    // Assert
    expect(await umbracoApi.script.doesExist(scriptPath)).toBeTruthy();

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test('can update a script', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const newContent = 'Howdy';
    scriptPath = await umbracoApi.script.create(scriptName, 'test');
    const script = await umbracoApi.script.get(scriptPath);
    script.content = newContent;

    // Act
    await umbracoApi.script.update(script);

    // Assert
    // Checks if the content was updated for the script
    const updatedScript = await umbracoApi.script.get(scriptPath);
    expect(updatedScript.content).toEqual(newContent);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test('can delete a script', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    scriptPath = await umbracoApi.script.create(scriptName, 'test');
    expect(await umbracoApi.script.doesExist(scriptPath)).toBeTruthy();

    // Act
    await umbracoApi.script.delete(scriptPath);

    // Assert
    expect(await umbracoApi.script.doesExist(scriptPath)).toBeFalsy();
  });
});
