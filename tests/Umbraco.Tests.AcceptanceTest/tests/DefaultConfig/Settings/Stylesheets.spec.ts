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
    await umbracoUi.stylesheet.clickOnThreeDotsBesideText("Stylesheets");
    await umbracoUi.stylesheet.clickOnNewStylesheetFileButton();
    await umbracoUi.stylesheet.enterStylesheetName(styleSheetName);
    await umbracoUi.stylesheet.clickOnSaveButton();

    // Assert
    expect(await umbracoApi.stylesheet.doesExists(styleSheetFileName)).toBeTruthy;
    // TODO: when frontend is ready, verify the new stylesheet is displayed under the Stylesheets section 
    // TODO: when frontend is ready, verify the notification displays
    
    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);
  });

  test('can create a new Rich Text Editor style sheet file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);

    //Act
    await umbracoUi.stylesheet.clickOnThreeDotsBesideText("Stylesheets");
    await umbracoUi.stylesheet.clickOnNewRTEStylesheetFileButton();
    await umbracoUi.stylesheet.enterStylesheetName(styleSheetName);
    await umbracoUi.stylesheet.addANewRule(ruleName, 'h1', 'color:red');
    await umbracoUi.stylesheet.clickOnSaveButton();

    // Assert
    expect(await umbracoApi.stylesheet.doesExists(styleSheetFileName)).toBeTruthy;
    expect(await umbracoApi.stylesheet.doesRuleNameExists(styleSheetFileName, ruleName)).toBeTruthy;
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
    await umbracoUi.stylesheet.openAStylesheetFile(styleSheetFileName);
    await umbracoUi.stylesheet.addANewRule(ruleName, 'h1', 'color:red');
    await umbracoUi.stylesheet.clickOnSaveButton();

    // Assert
    expect(await umbracoApi.stylesheet.doesRuleNameExists(styleSheetFileName, ruleName)).toBeTruthy;
    // TODO: when frontend is ready, verify the notification displays
      
    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);
  });

  test('can delete a stylesheet file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleSheetFileName);
    await umbracoApi.stylesheet.create(styleSheetFileName, '', '/');

    //Act
    await umbracoUi.stylesheet.clickOnCaretButton();
    await umbracoUi.stylesheet.clickOnThreeDotsBesideText(styleSheetFileName);
    await umbracoUi.stylesheet.deleteStylesheetFile();

    // Assert
    expect(await umbracoApi.stylesheet.doesNameExists(styleSheetFileName)).toBeFalsy();
    // TODO: when frontend is ready, verify the new stylesheet is NOT displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays
  });
  
  test('can create a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);  

    // Act
    await umbracoUi.stylesheet.clickOnThreeDotsBesideText("Stylesheets");
    await umbracoUi.stylesheet.createNewFolder(styleFolderName);

    // Assert
    expect(await umbracoApi.stylesheet.doesFolderExists(styleFolderName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new folder is displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);
  });

  test('can remove a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);  
    await umbracoApi.stylesheet.createFolder(styleFolderName, '');

    // Act
    await umbracoUi.stylesheet.clickOnCaretButton();
    await umbracoUi.stylesheet.clickOnThreeDotsBesideText(styleFolderName);
    await umbracoUi.stylesheet.removeFolder();

    // Assert
    expect(await umbracoApi.stylesheet.doesFolderExists(styleFolderName)).toBeFalsy();
    // TODO: when frontend is ready, verify the removed folder is NOT displayed under the Stylesheets section
    // TODO: when frontend is ready, verify the notification displays
  });

  // TODO: remove skip when frontend is ready as currently it will create 2 child folders in UI when creating a folder in a folder
  test.skip('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.ensureNameNotExists(styleFolderName);
    await umbracoApi.stylesheet.createFolder(styleFolderName);
    const childsFolderName = "ChildFolderName";

    // Act
    await umbracoUi.stylesheet.clickOnCaretButton();
    await umbracoUi.stylesheet.clickOnThreeDotsBesideText(styleFolderName);
    await umbracoUi.stylesheet.createNewFolder(childsFolderName);

    //Assert 
    expect(await umbracoApi.stylesheet.doesNameExists(childsFolderName)).toBeTruthy();
    const styleChildren = await umbracoApi.stylesheet.getChildren(styleFolderName);
    expect(styleChildren[0].path).toBe(styleFolderName + '/' + childsFolderName);
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
    await umbracoUi.stylesheet.clickOnCaretButton();
    await umbracoUi.stylesheet.clickOnCaretButtonBeforeText(styleFolderName);
    await umbracoUi.stylesheet.clickOnThreeDotsBesideText(childFolderName);
    await umbracoUi.stylesheet.createNewFolder(childOfChildFolderName);

    //Assert 
    expect(await umbracoApi.stylesheet.doesNameExists(childOfChildFolderName)).toBeTruthy();
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
