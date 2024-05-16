import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Stylesheets tests', () => {
  const stylesheetName = 'TestStyleSheetFile.css';
  const styleName = 'TestStyleName';
  const styleSelector = 'h1';
  const styleStyles = 'color:red';

  test.beforeEach(async ({umbracoUi,umbracoApi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  test('can create a empty stylesheet @smoke', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickCreateButton();
    await umbracoUi.stylesheet.clickNewStylesheetButton();
    await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
    await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName);
  });

  test('can create a stylesheet with content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const stylesheetContent = 'TestContent';

    //Act
    await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickCreateButton();
    await umbracoUi.stylesheet.clickNewStylesheetButton();
    await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
    await umbracoUi.stylesheet.enterStylesheetContent(stylesheetContent);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
    const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
    expect(stylesheetData.content).toEqual(stylesheetContent);
    await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName);
  });

  test.skip('can create a new Rich Text Editor stylesheet file @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const stylesheetContent = '/**umb_name:' + styleName + '*/\n' + styleSelector + ' {\n\t' +  styleStyles + '\n}';

    //Act
    await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.clickCreateButton();
    await umbracoUi.stylesheet.clickNewRichTextEditorStylesheetButton();
    await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
    await umbracoUi.stylesheet.addRTEStyle(styleName, styleSelector, styleStyles);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeTruthy();
    const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
    expect(stylesheetData.content).toEqual(stylesheetContent);
    await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName);
  });

  test.skip('can update a stylesheet @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const stylesheetContent = '/**umb_name:' + styleName + '*/\n' + styleSelector + ' {\n\t' +  styleStyles + '\n}';
    await umbracoApi.stylesheet.create(stylesheetName, '', '/');
    expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeTruthy();

    //Act
    await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.stylesheet.openStylesheetByNameAtRoot(stylesheetName);
    await umbracoUi.stylesheet.addRTEStyle(styleName, styleSelector, styleStyles);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
    expect(stylesheetData.content).toEqual(stylesheetContent);
  });

  test('can delete a stylesheet @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.stylesheet.create(stylesheetName, '', '/');

    //Act
    await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.stylesheet.reloadStylesheetTree();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetName);
    await umbracoUi.stylesheet.clickDeleteAndConfirmButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeFalsy();
    await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName, false);
  });

  test('can rename a stylesheet @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongStylesheetName = 'WrongStylesheetName.css';
    await umbracoApi.stylesheet.ensureNameNotExists(wrongStylesheetName);
    await umbracoApi.stylesheet.create(wrongStylesheetName, '', '/');

    //Act
    await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.stylesheet.reloadStylesheetTree();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(wrongStylesheetName);
    await umbracoUi.stylesheet.rename(stylesheetName);

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
    expect(await umbracoApi.stylesheet.doesNameExist(wrongStylesheetName)).toBeFalsy();
  });

  test('can edit rich text editor styles', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const newStyleName = 'TestNewStyleName';
    const newStyleSelector = 'h2';
    const newStyleStyles = 'color: white';
    const newStylesheetContent = '/**umb_name:' + newStyleName + '*/\n' + newStyleSelector + ' {\n\t' +  newStyleStyles + '\n}';
    const stylesheetContent = '/**umb_name:' + styleName + '*/\n' + styleSelector + ' {\n\t' +  styleStyles + '\n}';
    await umbracoApi.stylesheet.create(stylesheetName, stylesheetContent, '/');
    expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeTruthy();

    //Act
    await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.stylesheet.openStylesheetByNameAtRoot(stylesheetName);
    await umbracoUi.stylesheet.editRTEStyle(styleName, newStyleName, newStyleSelector, newStyleStyles);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
    expect(stylesheetData.content).toEqual(newStylesheetContent);
  });

  test.skip('can remove rich text editor styles', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const stylesheetContent = '/**umb_name:' + styleName + '*/\n' + styleSelector + ' {\n\t' +  styleStyles + '\n}';
    await umbracoApi.stylesheet.create(stylesheetName, stylesheetContent, '/');
    expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeTruthy();

    //Act
    await umbracoUi.stylesheet.openStylesheetByNameAtRoot(stylesheetName);
    await umbracoUi.stylesheet.removeRTEStyle(styleName);
    await umbracoUi.stylesheet.clickSaveButton();

    // Assert
    await umbracoUi.stylesheet.isSuccessNotificationVisible();
    const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
    expect(stylesheetData.content).toEqual('');
  });
});
