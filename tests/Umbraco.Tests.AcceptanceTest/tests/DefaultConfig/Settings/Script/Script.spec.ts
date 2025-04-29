import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const scriptName = 'TestScript.js';
const scriptPath = '/' + scriptName;

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoApi.script.ensureNameNotExists(scriptName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.script.ensureNameNotExists(scriptName);
});

test('can create a empty script', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.script.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.script.clickActionsMenuAtRoot();
  await umbracoUi.script.clickCreateButton();
  await umbracoUi.script.clickNewJavascriptFileButton();
  await umbracoUi.script.enterScriptName(scriptName);
  await umbracoUi.script.clickSaveButton();

  // Assert
  //await umbracoUi.script.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.script.isErrorNotificationVisible(false);
  expect(await umbracoApi.script.doesNameExist(scriptName)).toBeTruthy();
  await umbracoUi.script.isScriptRootTreeItemVisible(scriptName);
});

test('can create a script with content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const scriptContent = 'TestContent';
  await umbracoUi.script.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.script.clickActionsMenuAtRoot();
  await umbracoUi.script.clickCreateButton();
  await umbracoUi.script.clickNewJavascriptFileButton();
  await umbracoUi.script.enterScriptName(scriptName);
  await umbracoUi.script.enterScriptContent(scriptContent);
  await umbracoUi.script.clickSaveButton();

  // Assert
  //await umbracoUi.script.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.script.isErrorNotificationVisible(false);
  expect(await umbracoApi.script.doesNameExist(scriptName)).toBeTruthy();
  const scriptData = await umbracoApi.script.getByName(scriptName);
  expect(scriptData.content).toBe(scriptContent);
  await umbracoUi.script.isScriptRootTreeItemVisible(scriptName);
});

test('can update a script', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.script.create(scriptName, '');
  const updatedScriptContent = 'const test = {\r\n    script = \u0022Test\u0022,\r\n    extension = \u0022.js\u0022,\r\n    scriptPath: function() {\r\n        return this.script \u002B this.extension;\r\n    }\r\n};\r\n';
  await umbracoUi.script.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.script.openScriptAtRoot(scriptName);
  await umbracoUi.script.enterScriptContent(updatedScriptContent);
  await umbracoUi.script.clickSaveButton();

  // Assert
  //await umbracoUi.script.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.script.isErrorNotificationVisible(false);
  const updatedScript = await umbracoApi.script.get(scriptPath);
  expect(updatedScript.content).toBe(updatedScriptContent);
});

test('can delete a script', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.script.create(scriptName, '');
  await umbracoUi.script.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.script.reloadScriptTree();
  await umbracoUi.script.clickActionsMenuForScript(scriptName);
  await umbracoUi.script.clickDeleteAndConfirmButton();

  // Assert
  //await umbracoUi.script.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.script.isErrorNotificationVisible(false);
  expect(await umbracoApi.script.doesNameExist(scriptName)).toBeFalsy();
  await umbracoUi.script.isScriptRootTreeItemVisible(scriptName, false, false);
});

test('can rename a script', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongScriptName = 'WrongTestScript.js';
  await umbracoApi.script.ensureNameNotExists(wrongScriptName);
  await umbracoApi.script.create(wrongScriptName, '');
  await umbracoUi.script.goToScript(wrongScriptName);

  // Act
  await umbracoUi.script.clickActionsMenuForScript(wrongScriptName);
  await umbracoUi.script.rename(scriptName);

  // Assert
  await umbracoUi.script.isErrorNotificationVisible(false);
  expect(await umbracoApi.script.doesNameExist(scriptName)).toBeTruthy();
  expect(await umbracoApi.script.doesNameExist(wrongScriptName)).toBeFalsy();
});

test('cannot create a script with an empty name', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.script.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.script.clickActionsMenuAtRoot();
  await umbracoUi.script.clickCreateButton();
  await umbracoUi.script.clickNewJavascriptFileButton();
  await umbracoUi.script.clickSaveButton();

  // Assert
  // TODO: Uncomment this when the front-end is ready. Currently there is no error displays.
  //await umbracoUi.script.isErrorNotificationVisible();
  expect(await umbracoApi.script.doesNameExist(scriptName)).toBeFalsy();
});
