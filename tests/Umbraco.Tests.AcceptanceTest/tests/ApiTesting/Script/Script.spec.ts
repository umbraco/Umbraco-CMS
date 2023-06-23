import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Script tests', () => {
  const scriptName = 'scriptName.js';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.ensureScriptNotNameNotExistsAtRoot(scriptName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.script.ensureScriptNotNameNotExistsAtRoot(scriptName);
  });

  test('can create a script', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.script.createScript(scriptName, 'test');

    // Assert
    await expect(await umbracoApi.script.doesScriptWithNameExistAtRoot(scriptName)).toBeTruthy();
  });

  test('can update a script', async ({page, umbracoApi, umbracoUi}) => {
    const newContent = 'Howdy';

    await umbracoApi.script.createScript(scriptName, 'test');

    const script = await umbracoApi.script.getScriptByNameAtRoot(scriptName);

    script.content = newContent;

    await umbracoApi.script.updateScript(script);

    // Assert
    // Checks if the content was updated for the script
    const updatedScript = await umbracoApi.script.getScriptByPath(script.path);
    await expect(updatedScript.content === newContent).toBeTruthy();
  });

  test('can delete a script', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.script.createScript(scriptName, 'test');

    await expect(await umbracoApi.script.doesScriptWithNameExistAtRoot(scriptName)).toBeTruthy();

    const script = await umbracoApi.script.getScriptByNameAtRoot(scriptName);

    await umbracoApi.script.deleteScriptByPath(script.path);

    // Assert
    await expect(await umbracoApi.script.doesScriptWithNameExistAtRoot(scriptName)).toBeFalsy();
  });
});
