import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('Script tests', () => {
  const scriptName = 'TestScript';
  const scriptPath = scriptName + '.js';
  const scriptFolderName = 'TestScriptFolder';

  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.uiBaseLocators.goToSection(ConstantHelper.sections.settings);
  });

  test('can create a empty script', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptPath);

    // Act
    await umbracoUi.script.clickActionsMenuAtRoot();
    await umbracoUi.script.clickNewScriptButton();
    await umbracoUi.script.enterScriptName(scriptName);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.script.clickSaveButton();

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(scriptPath)).toBeTruthy();

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptPath);
  });

  test('can update a script', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptPath);
    await umbracoApi.script.create(scriptPath, 'test');
    const updatedScriptContent = 'const test = {\r\n    script = \u0022Test\u0022,\r\n    extension = \u0022.js\u0022,\r\n    scriptPath: function() {\r\n        return this.script \u002B this.extension;\r\n    }\r\n};\r\n';

    // Act
    await umbracoUi.script.openScriptAtRoot(scriptPath);
    await umbracoUi.script.enterScriptContent(updatedScriptContent);
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.script.clickSaveButton();

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    const updatedScript = await umbracoApi.script.get(scriptPath);
    expect(updatedScript.content).toBe(updatedScriptContent);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptPath);
  });

  test('can delete a script', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptPath);
    await umbracoApi.script.create(scriptPath, '');

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptPath);
    await umbracoUi.script.deleteScript();

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(scriptPath)).toBeFalsy();
  });

  // Folder
  test.skip('can create a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);

    // Act
    await umbracoUi.script.clickActionsMenuAtRoot();
    await umbracoUi.script.createFolder(scriptFolderName);

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    // TODO: Use the reload function for scripts when it is implemented
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeTruthy();

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptPath);
  });

  test.skip('can delete a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.createFolder(scriptFolderName);

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.deleteFolder();

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    // TODO: Use the reload function for scripts when it is implemented
    await page.reload();
    await umbracoUi.uiBaseLocators.goToSection(ConstantHelper.sections.settings);
    // await page.locator('umb-tree-item', {hasText: 'Scripts'}).locator('#caret-button').click();
    expect(umbracoUi.uiBaseLocators.isTreeItemVisible(scriptFolderName)).not.toBeTruthy();

    // await expect(page.locator('umb-tree-item').locator('[label="' + scriptFolderName + '"] ')).not.toBeVisible();
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeFalsy();
  });

  test.skip('can create a script in a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    const scriptContent = 'const test = {\r\n    script = \u0022Test\u0022,\r\n    extension = \u0022.js\u0022,\r\n    scriptPath: function() {\r\n        return this.script \u002B this.extension;\r\n    }\r\n};\r\n';

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.clickNewScriptButton();
    await umbracoUi.script.enterScriptName(scriptName);
    await umbracoUi.script.enterScriptContent(scriptContent);
    await page.waitForTimeout(1000);
    await umbracoUi.script.clickSaveButton();

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    // TODO: Check if the script was created correctly in the UI when the refresh button is implemented
    expect(await umbracoApi.script.doesNameExist(scriptPath)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren(scriptFolderName);
    expect(scriptChildren[0].path).toBe(scriptFolderName + '/' + scriptPath);
    const scriptData = await umbracoApi.script.get(scriptChildren[0].path);
    expect(scriptData.content).toBe(scriptContent);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test.skip('can create a folder in a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    const childFolderName = "childFolderName";

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.createFolder(childFolderName);

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    // TODO: Check if the folder was created correctly in the UI when the refresh button is implemented
    expect(await umbracoApi.script.doesNameExist(childFolderName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren(scriptFolderName);
    expect(scriptChildren[0].path).toBe(scriptFolderName + '/' + childFolderName);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test.skip('can create a folder in a folder in a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    const childOfChildFolderName = 'ChildOfChildFolderName';
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    await umbracoApi.script.createFolder(childFolderName, scriptFolderName);

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.createFolder(childFolderName);

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    // TODO: Check if the folder was created correctly in the UI when the refresh button is implemented
    expect(await umbracoApi.script.doesNameExist(childOfChildFolderName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren(scriptFolderName + '/' + childFolderName);
    expect(scriptChildren[0].path).toBe(scriptFolderName + '/' + childFolderName + '/' + childOfChildFolderName);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  // TODO: Remove skip from this test when the frontend is able to create a script in a folder in a folder. Currently the script is created in the first folder.
  test.skip('can create a script in a folder in a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    await umbracoApi.script.createFolder(childFolderName, scriptFolderName);

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.clickNewScriptButton();
    await umbracoUi.script.enterScriptName(scriptName);
    await page.waitForTimeout(1000);
    await umbracoUi.script.clickSaveButton();

    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await umbracoUi.template.clickSaveButton();

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    // TODO: Check if the script was created correctly in the UI when the refresh button is implemented
    expect(await umbracoApi.script.doesNameExist(scriptPath)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren(scriptFolderName + '/' + childFolderName);
    expect(scriptChildren[0].path).toBe(scriptFolderName + '/' + childFolderName + '/' + scriptPath);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });
});
