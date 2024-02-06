import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Script tests', () => {
  const scriptName = 'TestScript.js';
  const scriptPath = '/' + scriptName;

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.script.goToSection(ConstantHelper.sections.settings);
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test('can create a empty script', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.script.clickActionsMenuAtRoot();
    await umbracoUi.script.clickCreateThreeDotsButton();
    await umbracoUi.script.clickNewJavascriptFileButton();
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.script.enterScriptName(scriptName);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.script.clickSaveButton();

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(scriptName)).toBeTruthy();
  });

  test('can create a script with content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const scriptContent = 'TestContent';

    // Act
    await umbracoUi.script.clickActionsMenuAtRoot();
    await umbracoUi.script.clickCreateThreeDotsButton();
    await umbracoUi.script.clickNewJavascriptFileButton();
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.script.enterScriptName(scriptName);
    await umbracoUi.script.enterScriptContent(scriptContent);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.script.clickSaveButton();

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(scriptName)).toBeTruthy();
    const scriptData = await umbracoApi.script.getByName(scriptName);
    expect(scriptData.content).toBe(scriptContent);
  });

  test('can update a script', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.create(scriptName, 'test');
    const updatedScriptContent = 'const test = {\r\n    script = \u0022Test\u0022,\r\n    extension = \u0022.js\u0022,\r\n    scriptPath: function() {\r\n        return this.script \u002B this.extension;\r\n    }\r\n};\r\n';

    // Act
    await umbracoUi.script.openScriptAtRoot(scriptName);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.script.enterScriptContent(updatedScriptContent);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.script.clickSaveButton();

    // Assert
    // TODO: Uncomment when the notification is visible. Currently there is no notification after you update a script
    // await umbracoUi.script.isSuccessNotificationVisible();
    const updatedScript = await umbracoApi.script.get(scriptPath);
    expect(updatedScript.content).toBe(updatedScriptContent);
  });

  test('can delete a script', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.create(scriptName, '');

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptName);
    await umbracoUi.script.deleteScript();

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(scriptName)).toBeFalsy();
  });
});
