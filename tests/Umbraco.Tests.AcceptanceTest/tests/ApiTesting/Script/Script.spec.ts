import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Script tests', () => {
  let scriptPath = "";
  const scriptName = 'scriptName.js';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExistsAtRoot(scriptName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.delete(scriptPath);
  });

  test('can create a script', async ({page, umbracoApi, umbracoUi}) => {
    scriptPath = await umbracoApi.script.create(scriptName, 'test');

    // Assert
    await expect(await umbracoApi.script.exists(scriptPath)).toBeTruthy();
  });

  test('can update a script', async ({page, umbracoApi, umbracoUi}) => {
    const newContent = 'Howdy';

    scriptPath = await umbracoApi.script.create(scriptName, 'test');

    const script = await umbracoApi.script.get(scriptPath);

    script.content = newContent;

    await umbracoApi.script.update(script);

    // Assert
    // Checks if the content was updated for the script
    const updatedScript = await umbracoApi.script.get(scriptPath);
    await expect(updatedScript.content).toEqual(newContent);
  });

  test('can delete a script', async ({page, umbracoApi, umbracoUi}) => {
    scriptPath = await umbracoApi.script.create(scriptName, 'test');

    await expect(await umbracoApi.script.exists(scriptPath)).toBeTruthy();

    await umbracoApi.script.delete(scriptPath);

    // Assert
    await expect(await umbracoApi.script.exists(scriptPath)).toBeFalsy();
  });
});
