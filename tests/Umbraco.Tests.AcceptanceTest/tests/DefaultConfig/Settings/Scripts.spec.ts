import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Script tests', () => {
  const scriptName = 'TestScript.js';
  const scriptPath = '/' + scriptName;
  const scriptFolderName = 'TestScriptFolder';

  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.script.goToSection(ConstantHelper.sections.settings);
  });

  test('can create a empty script', async ({ umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptName);

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

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test('can create a script with content', async ({ umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptName);
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

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test('can update a script', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptName);
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
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.script.isSuccessNotificationVisible();
    const updatedScript = await umbracoApi.script.get(scriptPath);
    expect(updatedScript.content).toBe(updatedScriptContent);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test('can delete a script', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptName);
    await umbracoApi.script.create(scriptName, '');

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptName);
    await umbracoUi.script.deleteScript();

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(scriptName)).toBeFalsy();
  });

  // Folder
  test('can create a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);

    // Act
    await umbracoUi.script.clickActionsMenuAtRoot();
    await umbracoUi.script.createFolder(scriptFolderName);

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeTruthy();

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  test('can delete a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.deleteFolder();

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeFalsy();
  });

  test('can create a script in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    const scriptContent = 'const test = {\r\n    script = \u0022Test\u0022,\r\n    extension = \u0022.js\u0022,\r\n    scriptPath: function() {\r\n        return this.script \u002B this.extension;\r\n    }\r\n};\r\n';

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
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
    // TODO: Check if the script was created correctly in the UI when the refresh button is implemented
    expect(await umbracoApi.script.doesNameExist(scriptName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren('/' + scriptFolderName);
    expect(scriptChildren[0].path).toBe('/' + scriptFolderName + '/' + scriptName);
    const scriptData = await umbracoApi.script.get(scriptChildren[0].path);
    expect(scriptData.content).toBe(scriptContent);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    const childFolderName = 'childFolderName';

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.createFolder(childFolderName);

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    // TODO: Check if the folder was created correctly in the UI when the refresh button is implemented
    expect(await umbracoApi.script.doesNameExist(childFolderName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren('/' + scriptFolderName);
    expect(scriptChildren[0].path).toBe('/' + scriptFolderName + '/' + childFolderName);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test('can create a folder in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    const childOfChildFolderName = 'ChildOfChildFolderName';
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    await umbracoApi.script.createFolder(childFolderName, scriptFolderName);

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickCaretButtonForName(scriptFolderName);
    await umbracoUi.script.clickActionsMenuForScript(childFolderName);
    await umbracoUi.script.createFolder(childOfChildFolderName);

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    // TODO: Check if the folder was created correctly in the UI when the refresh button is implemented
    expect(await umbracoApi.script.doesNameExist(childOfChildFolderName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren('/' + scriptFolderName + '/' + childFolderName);
    expect(scriptChildren[0].path).toBe('/' + scriptFolderName + '/' + childFolderName + '/' + childOfChildFolderName);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test('can create a script in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    await umbracoApi.script.createFolder(childFolderName, scriptFolderName);

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickCaretButtonForName(scriptFolderName);
    await umbracoUi.script.clickActionsMenuForScript(childFolderName);
    await umbracoUi.script.clickCreateThreeDotsButton();
    await umbracoUi.script.clickNewJavascriptFileButton();
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.script.enterScriptName(scriptName);
    await umbracoUi.script.enterScriptName(scriptName);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.script.clickSaveButton();

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    // TODO: Check if the script was created correctly in the UI when the refresh button is implemented
    expect(await umbracoApi.script.doesNameExist(scriptName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren('/' + scriptFolderName + '/' + childFolderName);
    expect(scriptChildren[0].path).toBe('/' + scriptFolderName + '/' + childFolderName + '/' + scriptName);

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });
});
