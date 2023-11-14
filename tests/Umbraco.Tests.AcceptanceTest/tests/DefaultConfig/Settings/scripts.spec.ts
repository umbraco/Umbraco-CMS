import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('Script tests', () => {

  const scriptName = 'TestScript';
  const scriptPath = scriptName + '.js';
  const scriptFolderName = 'TestScriptFolder';

  test('can create a script', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptPath);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Scripts'}).getByLabel('Open actions menu').click({force: true});
    await page.getByLabel('New empty script').click();

    // TODO: Change the label to script name when the label is updated
    await page.getByLabel('template name').fill(scriptName);
    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await page.getByLabel('Save').click({force: true});

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
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToScript(scriptPath)
    await page.locator('textarea.inputarea').clear();
    await page.locator('textarea.inputarea').fill(updatedScriptContent);
    await page.getByLabel('Save').click();

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
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Scripts'}).locator('#caret-button').click();
    await page.locator('umb-tree-item').locator('[label="' + scriptPath + '"] >> [label="Open actions menu"]').click();
    await page.getByLabel('Delete').click();
    await page.locator('#confirm').getByLabel('Delete').click();

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    expect(await umbracoApi.script.doesNameExist(scriptPath)).toBeFalsy();
  });

  // Folder
  test('can create a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Scripts'}).getByLabel('Open actions menu').click({force: true});
    await page.getByLabel('Create folder').click();
    await page.locator('[headline="Create Folder"] >> input').fill(scriptFolderName);
    await page.getByLabel('Create Folder', {exact: true}).click();

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    // TODO: Use the reload function for scripts when it is implemented
    await page.reload();
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Scripts'}).locator('#caret-button').click();
    await expect(page.locator('umb-tree-item').locator('[label="' + scriptFolderName + '"] ')).toBeVisible();
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeTruthy();

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptPath);
  });

  test('can delete a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.createFolder(scriptFolderName);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Scripts'}).locator('#caret-button').click();
    await page.locator('umb-tree-item').locator('[label="' + scriptFolderName + '"] >> [label="Open actions menu"]').click();
    await page.getByLabel('Remove folder').click();
    await page.locator('#confirm').getByLabel('Delete').click();

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    // TODO: Use the reload function for scripts when it is implemented
    await page.reload();
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Scripts'}).locator('#caret-button').click();
    await expect(page.locator('umb-tree-item').locator('[label="' + scriptFolderName + '"] ')).not.toBeVisible();
    expect(await umbracoApi.script.doesFolderExist(scriptFolderName)).toBeFalsy();
  });

  test('can create a script in a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Scripts'}).locator('#caret-button').click();
    await page.locator('umb-tree-item').locator('[label="' + scriptFolderName + '"] >> [label="Open actions menu"]').click();
    await page.getByLabel('New empty script').click();

    // TODO: Change the label to script name when the label is updated
    await page.getByLabel('template name').fill(scriptName);
    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await page.getByLabel('Save').click({force: true});

    // Assert
    // TODO: Uncomment when the notification is visible
    // await umbracoUi.isSuccessNotificationVisible();
    // TODO: Check if the script was created correctly in the UI when the refresh button is implemented
    expect(await umbracoApi.script.doesNameExist(scriptPath)).toBeTruthy();
    const scriptChildren = await umbracoApi.script.getChildren(scriptFolderName);
    expect(scriptChildren[0].path).toBe(scriptFolderName + '/' + scriptPath);
    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  });

  test('can create a folder in a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    const childFolderName = "childFolderName";

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Scripts'}).locator('#caret-button').click();
    await page.locator('umb-tree-item').locator('[label="' + scriptFolderName + '"] >> [label="Open actions menu"]').click();

    await page.getByLabel('Create folder').click();
    await page.locator('[headline="Create Folder"] >> input').fill(childFolderName);
    await page.getByLabel('Create Folder', {exact: true}).click();

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

  // TODO:
  test('can create a folder in a folder in a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    const childOfChildFolderName = 'ChildOfChildFolderName';
    await umbracoApi.script.ensureNameNotExists(scriptFolderName);
    await umbracoApi.script.createFolder(scriptFolderName);
    await umbracoApi.script.createFolder(childFolderName, scriptFolderName);

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Scripts'}).locator('#caret-button').click();
    await page.locator('umb-tree-item >> [label="' + scriptFolderName + '"]').locator('#caret-button').click();
    await page.locator('umb-tree-item').locator('[label="' + childFolderName + '"] >> [label="Open actions menu"]').click();

    await page.getByLabel('Create folder').click();
    await page.locator('[headline="Create Folder"] >> input').fill(childOfChildFolderName);
    await page.getByLabel('Create Folder', {exact: true}).click();

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

  // TODO: Uncomment this test when the frontend is able to create a script in a folder in a folder. Currently the script is created in the first folder.
  // test('can create a script in a folder in a folder', async ({page, umbracoApi, umbracoUi}) => {
  //   // Arrange
  //   const childFolderName = 'ChildFolderName';
  //   await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  //   await umbracoApi.script.createFolder(scriptFolderName);
  //   await umbracoApi.script.createFolder(childFolderName, scriptFolderName);
  //
  //   // Act
  //   await page.goto(umbracoApi.baseUrl + '/umbraco');
  //   await umbracoUi.goToSection(ConstantHelper.sections.settings);
  //   await page.locator('umb-tree-item', {hasText: 'Scripts'}).locator('#caret-button').click();
  //   await page.locator('umb-tree-item >> [label="' + scriptFolderName + '"]').locator('#caret-button').click();
  //   await page.locator('umb-tree-item').locator('[label="' + childFolderName + '"] >> [label="Open actions menu"]').click();
  //   await page.getByLabel('New empty script').click();
  //   // TODO: Change the label to script name when the label is updated
  //   await page.getByLabel('template name').fill(scriptName);
  //   // TODO: Remove this timeout when frontend validation is implemented
  //   await page.waitForTimeout(1000);
  //   await page.getByLabel('Save').click({force: true});
  //
  //   // Assert
  //   // TODO: Uncomment when the notification is visible
  //   // await umbracoUi.isSuccessNotificationVisible();
  //   // TODO: Check if the script was created correctly in the UI when the refresh button is implemented
  //   expect(await umbracoApi.script.doesNameExist(scriptPath)).toBeTruthy();
  //   const scriptChildren = await umbracoApi.script.getChildren(scriptFolderName + '/' + childFolderName);
  //   expect(scriptChildren[0].path).toBe(scriptFolderName + '/' + childFolderName + '/' + scriptPath);
  //
  //   // Clean
  //   await umbracoApi.script.ensureNameNotExists(scriptFolderName);
  // });
});
