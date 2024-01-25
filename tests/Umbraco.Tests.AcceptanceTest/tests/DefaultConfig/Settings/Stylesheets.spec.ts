import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Stylesheets tests', () => {
  const stylesheetName = 'TestStyleSheetFile.css';
  const ruleName = 'TestRuleName';
  const stylesheetFolderName = 'TestStylesheetFolder';

  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
  });

  test('can create a empty stylesheet', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);

    //Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickCreateButton();
    await umbracoUi.stylesheet.clickNewStylesheetButton();
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new stylesheet is displayed under the Stylesheets section

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test('can create a stylesheet with content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
    const stylesheetContent = 'TestContent';

    //Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickCreateButton();
    await umbracoUi.stylesheet.clickNewStylesheetButton();
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
    await umbracoUi.stylesheet.enterStylesheetContent(stylesheetContent);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new stylesheet is displayed under the Stylesheets section
    const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
    expect(stylesheetData.content).toEqual(stylesheetContent);

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  //  We are not able to create stylesheet with RTE styles.
  test.skip('can create a new Rich Text Editor style sheet file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);

    //Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickCreateButton();
    await umbracoUi.stylesheet.clickNewRichTextEditorStylesheetButton();
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
    await umbracoUi.stylesheet.addNewRule(ruleName, 'h1', 'color:red');
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeTruthy();
    expect(await umbracoApi.stylesheet.doesRuleNameExist(stylesheetName, ruleName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new stylesheet is displayed under the Stylesheets section

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  // We are not able to create stylesheet with RTE styles.
  test.skip('can update a stylesheet', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
    await umbracoApi.stylesheet.create(stylesheetName, '', '/');

    //Act
    await umbracoUi.stylesheet.openStylesheetByNameAtRoot(stylesheetName);
    await umbracoUi.stylesheet.addNewRule(ruleName, 'h1', 'color:red');
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    // await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesRuleNameExist(stylesheetName, ruleName)).toBeTruthy();
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test('can delete a stylesheet', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
    await umbracoApi.stylesheet.create(stylesheetName, '', '/');

    //Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetName);
    await umbracoUi.stylesheet.deleteStylesheet();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeFalsy();
    // TODO: when frontend is ready, verify the new stylesheet is NOT displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays
  });

  test('can create a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);

    // Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.createFolder(stylesheetFolderName);

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetFolderName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new folder is displayed under the Stylesheets section

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
  });

  test('can delete a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
    await umbracoApi.stylesheet.createFolder(stylesheetFolderName, '');

    // Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetFolderName);
    await umbracoUi.stylesheet.deleteFolder();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();

    expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetFolderName)).toBeFalsy();
    // TODO: when frontend is ready, verify the removed folder is NOT displayed under the Stylesheets section
  });

  test('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
    await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
    const childFolderName = 'ChildFolderName';

    // Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetFolderName);
    await umbracoUi.stylesheet.createFolder(childFolderName);

    //Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(childFolderName)).toBeTruthy();
    const styleChildren = await umbracoApi.stylesheet.getChildren('/' + stylesheetFolderName);
    expect(styleChildren[0].path).toBe('/' + stylesheetFolderName + '/' + childFolderName);
    // TODO: when frontend is ready, verify the new folder is displayed under the Stylesheets section

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
  });

  test('can create a folder in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    const childOfChildFolderName = 'ChildOfChildFolderName';
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
    await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
    await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderName);

    // Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickCaretButtonForName(stylesheetFolderName);
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(childFolderName);
    await umbracoUi.stylesheet.createFolder(childOfChildFolderName);

    //Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(childOfChildFolderName)).toBeTruthy();
    const styleChildren = await umbracoApi.stylesheet.getChildren('/' + stylesheetFolderName + '/' + childFolderName);
    expect(styleChildren[0].path).toBe('/' + stylesheetFolderName + '/' + childFolderName + '/' + childOfChildFolderName);
    // TODO: when frontend is ready, verify the new folder is displayed under the Stylesheets section

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
  });

  test('can create a stylesheet in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
    await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
    const stylesheetContent = 'TestContent';

    //Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetFolderName);
    await umbracoUi.stylesheet.clickCreateButton();
    await umbracoUi.stylesheet.clickNewStylesheetButton();
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
    await umbracoUi.stylesheet.enterStylesheetContent(stylesheetContent);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new stylesheet is displayed under the Stylesheets section
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
    const stylesheetChildren = await umbracoApi.stylesheet.getChildren('/' + stylesheetFolderName);
    expect(stylesheetChildren[0].path).toBe('/' + stylesheetFolderName + '/' + stylesheetName);
    const stylesheetData = await umbracoApi.stylesheet.get(stylesheetChildren[0].path);
    expect(stylesheetData.content).toBe(stylesheetContent);

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test('can create a stylesheet in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
    await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
    await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderName);
    const stylesheetContent = 'TestContent';

    //Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickCaretButtonForName(stylesheetFolderName);
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(childFolderName);
    await umbracoUi.stylesheet.clickCreateButton();
    await umbracoUi.stylesheet.clickNewStylesheetButton();
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
    await umbracoUi.stylesheet.enterStylesheetContent(stylesheetContent);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(500);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new stylesheet is displayed under the Stylesheets section
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
    const stylesheetChildren = await umbracoApi.stylesheet.getChildren('/' + stylesheetFolderName + '/' + childFolderName);
    expect(stylesheetChildren[0].path).toBe('/' + stylesheetFolderName + '/' + childFolderName  + '/' + stylesheetName);
    const stylesheetData = await umbracoApi.stylesheet.get(stylesheetChildren[0].path);
    expect(stylesheetData.content).toBe(stylesheetContent);

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });
});
