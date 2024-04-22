import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Script tests', () => {
  const scriptName = 'TestScript.js';
  const scriptFolderName = 'TestScriptFolder';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.script.goToSection(ConstantHelper.sections.settings);
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test.skip('can create a folder @smoke', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.script.clickActionsMenuAtRoot();
    await umbracoUi.script.createFolder(scriptFolderName);

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeTruthy();
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.isScriptTreeItemVisible(scriptFolderName);
  });

  test.skip('can delete a folder @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.createFolder(scriptFolderName);

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.deleteFolder();

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeFalsy();
    await umbracoUi.script.isScriptTreeItemVisible(scriptFolderName, false);
  });

  test.skip('can create a script in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.createFolder(scriptFolderName);
    const scriptContent = 'const test = {\r\n    script = \u0022Test\u0022,\r\n    extension = \u0022.js\u0022,\r\n    scriptPath: function() {\r\n        return this.script \u002B this.extension;\r\n    }\r\n};\r\n';

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.clickCreateButton();
    await umbracoUi.script.clickNewJavascriptFileButton();
    await umbracoUi.script.enterScriptName(scriptName);
    await umbracoUi.script.enterScriptContent(scriptContent);
    await umbracoUi.script.clickSaveButton();

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(scriptName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren('/' + scriptFolderName);
    expect(scriptChildren[0].path).toBe('/' + scriptFolderName + '/' + scriptName);
    const scriptData = await umbracoApi.script.get(scriptChildren[0].path);
    expect(scriptData.content).toBe(scriptContent);
    await umbracoUi.stylesheet.clickCaretButtonForName(scriptFolderName);
    await umbracoUi.script.isScriptTreeItemVisible(scriptName);
  });

  test.skip('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.createFolder(scriptFolderName);
    const childFolderName = 'childFolderName';

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.createFolder(childFolderName);

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(childFolderName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren('/' + scriptFolderName);
    expect(scriptChildren[0].path).toBe('/' + scriptFolderName + '/' + childFolderName);
    await umbracoUi.stylesheet.clickCaretButtonForName(scriptFolderName);
    await umbracoUi.script.isScriptTreeItemVisible(childFolderName);
  });

  test.skip('can create a folder in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    const childOfChildFolderName = 'ChildOfChildFolderName';
    await umbracoApi.script.createFolder(scriptFolderName);
    await umbracoApi.script.createFolder(childFolderName, scriptFolderName);

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickCaretButtonForName(scriptFolderName);
    await umbracoUi.script.clickActionsMenuForScript(childFolderName);
    await umbracoUi.script.createFolder(childOfChildFolderName);

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(childOfChildFolderName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren('/' + scriptFolderName + '/' + childFolderName);
    expect(scriptChildren[0].path).toBe('/' + scriptFolderName + '/' + childFolderName + '/' + childOfChildFolderName);
    await umbracoUi.stylesheet.clickCaretButtonForName(childFolderName);
    await umbracoUi.script.isScriptTreeItemVisible(childOfChildFolderName);
  });

  test.skip('can create a script in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    await umbracoApi.script.createFolder(scriptFolderName);
    await umbracoApi.script.createFolder(childFolderName, scriptFolderName);

    // Act
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickCaretButtonForName(scriptFolderName);
    await umbracoUi.script.clickActionsMenuForScript(childFolderName);
    await umbracoUi.script.clickCreateButton();
    await umbracoUi.script.clickNewJavascriptFileButton();
    await umbracoUi.script.enterScriptName(scriptName);
    await umbracoUi.script.clickSaveButton();

    // Assert
    await umbracoUi.script.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(scriptName)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren('/' + scriptFolderName + '/' + childFolderName);
    expect(scriptChildren[0].path).toBe('/' + scriptFolderName + '/' + childFolderName + '/' + scriptName);
    await umbracoUi.stylesheet.clickCaretButtonForName(childFolderName);
    await umbracoUi.script.isScriptTreeItemVisible(scriptName);
  });
});
