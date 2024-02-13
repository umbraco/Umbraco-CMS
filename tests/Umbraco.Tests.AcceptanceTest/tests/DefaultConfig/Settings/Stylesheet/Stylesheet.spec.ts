import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Stylesheets tests', () => {
  const stylesheetName = 'TestStyleSheetFile.css';
  const ruleName = 'TestRuleName';

  test.beforeEach(async ({umbracoUi,umbracoApi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test('can create a empty stylesheet', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickCreateThreeDotsButton();
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
  });

  test('can create a stylesheet with content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const stylesheetContent = 'TestContent';

    //Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickCreateThreeDotsButton();
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
  });

  //  We are not able to create stylesheet with RTE styles.
  test.skip('can create a new Rich Text Editor stylesheet file', async ({umbracoApi, umbracoUi}) => {
    //Act
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickCreateThreeDotsButton();
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
  });

  // We are not able to update a stylesheet with RTE styles.
  test.skip('can update a stylesheet', async ({umbracoApi, umbracoUi}) => {
    // Arrange
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
  });

  test('can delete a stylesheet', async ({umbracoApi, umbracoUi}) => {
    // Arrange
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
});
