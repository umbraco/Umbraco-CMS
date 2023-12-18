import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Stylesheets tests', () => {

  const styleSheetName = 'TestStyleSheetFile';
  const styleSheetFileName = styleSheetName + ".css";
  const ruleName = 'TestRuleName';
  const styleFolderName = 'TestStylesheetFolder';

  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
  });

  test('can create a stylesheet file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);

    //Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickNewStylesheetFileButton();
    await umbracoUi.stylesheet.enterStylesheetName(styleSheetName);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    expect(await umbracoApi.stylesheet.doesExist(styleSheetFileName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new stylesheet is displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);
  });

  test('can create a new Rich Text Editor style sheet file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);

    //Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickNewRTEStylesheetFileButton();
    await umbracoUi.stylesheet.enterStylesheetName(styleSheetName);
    await umbracoUi.stylesheet.addNewRule(ruleName, 'h1', 'color:red');
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    expect(await umbracoApi.stylesheet.doesExist(styleSheetFileName)).toBeTruthy();
    expect(await umbracoApi.stylesheet.doesRuleNameExist(styleSheetFileName, ruleName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new stylesheet is displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);
  });

  test('can edit a stylesheet file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);
    await umbracoApi.stylesheet.create(styleSheetFileName, '', '/');

    //Act
    await umbracoUi.stylesheet.openStylesheetFileByNameAtRoot(styleSheetFileName);
    await umbracoUi.stylesheet.addNewRule(ruleName, 'h1', 'color:red');
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    expect(await umbracoApi.stylesheet.doesRuleNameExist(styleSheetFileName, ruleName)).toBeTruthy();
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);
  });

  test('can delete a stylesheet file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);
    await umbracoApi.stylesheet.create(styleSheetFileName, '', '/');

    //Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(styleSheetFileName);
    await umbracoUi.stylesheet.deleteStylesheetFile();

    // Assert
    expect(await umbracoApi.stylesheet.doesNameExist(styleSheetFileName)).toBeFalsy();
    // TODO: when frontend is ready, verify the new stylesheet is NOT displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays
  });

  test.skip('can create a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);

    // Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.createNewFolder(styleFolderName);

    // Assert
    expect(await umbracoApi.stylesheet.doesFolderExist(styleFolderName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new folder is displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);
  });

  test.skip('can delete a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);
    await umbracoApi.stylesheet.createFolder(styleFolderName, '');

    // Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(styleFolderName);
    await umbracoUi.stylesheet.deleteFolder();

    // Assert
    expect(await umbracoApi.stylesheet.doesFolderExist(styleFolderName)).toBeFalsy();
    // TODO: when frontend is ready, verify the removed folder is NOT displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays
  });

  // TODO: remove skip when frontend is ready as currently it will create 2 child folders in UI when creating a folder in a folder
  test.skip('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);
    await umbracoApi.stylesheet.createFolder(styleFolderName);
    const childFolderName = "ChildFolderName";

    // Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(styleFolderName);
    await umbracoUi.stylesheet.createNewFolder(childFolderName);

    //Assert
    expect(await umbracoApi.stylesheet.doesNameExist(childFolderName)).toBeTruthy();
    const styleChildren = await umbracoApi.stylesheet.getChildren(styleFolderName);
    expect(styleChildren[0].path).toBe(styleFolderName + '/' + childFolderName);
    // TODO: when frontend is ready, verify the new folder is displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);
  });

  // TODO: remove skip when frontend is ready as currently it will create 2 child folders in UI when creating a folder in a folder
  test.skip('can create a folder in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolderName';
    const childOfChildFolderName = 'ChildOfChildFolderName';
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);
    await umbracoApi.stylesheet.createFolder(styleFolderName);
    await umbracoApi.stylesheet.createFolder(childFolderName, styleFolderName);

    // Act
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickCaretButtonForName(styleFolderName);
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(childFolderName);
    await umbracoUi.stylesheet.createNewFolder(childOfChildFolderName);

    //Assert
    expect(await umbracoApi.stylesheet.doesNameExist(childOfChildFolderName)).toBeTruthy();
    const styleChildren = await umbracoApi.stylesheet.getChildren(styleFolderName + '/' + childFolderName);
    expect(styleChildren[0].path).toBe(styleFolderName + '/' + childFolderName + '/' + childOfChildFolderName);
    // TODO: when frontend is ready, verify the new folder is displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);
  });

  // TODO: implement this later as the frontend is missing now
  test.skip('can create a stylesheet in a folder', async ({umbracoApi, umbracoUi}) => {

  });

  // TODO: implement this later as the frontend is missing now
  test.skip('can create a stylesheet in a folder in a folder', async ({umbracoApi, umbracoUi}) => {

  });

});
