import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Script tests', () => {
  let scriptPath = "";
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

    // Assert
    expect(await umbracoApi.script.doesExist(scriptPath)).toBeTruthy();
  });

  test('can update a script', async ({umbracoApi}) => {
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
