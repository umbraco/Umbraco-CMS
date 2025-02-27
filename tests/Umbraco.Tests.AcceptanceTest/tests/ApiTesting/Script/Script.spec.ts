import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Script tests', () => {
  let scriptPath = '';
  const scriptName = 'scriptName.js';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test('can create a script', async ({umbracoApi}) => {
    // Act
    scriptPath = await umbracoApi.script.create(scriptName, 'test');
    await umbracoApi.script.get(scriptPath);

    // Assert
    expect(await umbracoApi.script.doesExist(scriptPath)).toBeTruthy();
  });

  test('can update script name', async ({umbracoApi}) => {
    // Arrange
    const oldName = 'RandomScriptName.js';
    await umbracoApi.script.ensureNameNotExists(oldName);
    const oldScriptPath = await umbracoApi.script.create(oldName, 'test');

    // Act
    scriptPath = await umbracoApi.script.updateName(oldScriptPath, scriptName);

    // Assert
    // Checks if the content was updated for the script
    const updatedScript = await umbracoApi.script.get(scriptPath);
    expect(updatedScript.name).toEqual(scriptName);
  });

  test('can update script content', async ({umbracoApi}) => {
    // Arrange
    const newContent = 'BetterContent';
    scriptPath = await umbracoApi.script.create(scriptName, 'test');
    await umbracoApi.script.get(scriptPath);

    // Act
    await umbracoApi.script.updateContent(scriptPath, newContent);

    // Assert
    // Checks if the content was updated for the script
    const updatedScript = await umbracoApi.script.get(scriptPath);
    expect(updatedScript.content).toEqual(newContent);
  });

  test('can delete a script', async ({umbracoApi}) => {
    // Arrange
    scriptPath = await umbracoApi.script.create(scriptName, 'test');
    expect(await umbracoApi.script.doesExist(scriptPath)).toBeTruthy();

    // Act
    await umbracoApi.script.delete(scriptPath);

    // Assert
    expect(await umbracoApi.script.doesExist(scriptPath)).toBeFalsy();
  });
});
