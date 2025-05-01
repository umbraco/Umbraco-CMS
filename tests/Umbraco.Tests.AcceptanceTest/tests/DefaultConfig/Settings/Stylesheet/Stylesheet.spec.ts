import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

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

test('can create a empty stylesheet', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.stylesheet.clickActionsMenuAtRoot();
  await umbracoUi.stylesheet.clickCreateButton();
  await umbracoUi.stylesheet.clickNewStylesheetButton();
  await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
  await umbracoUi.stylesheet.clickSaveButton();

  // Assert
  //await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.stylesheet.isErrorNotificationVisible(false);
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName);
});

test('can create a stylesheet with content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetContent = 'TestContent';
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  //Act
  await umbracoUi.stylesheet.clickActionsMenuAtRoot();
  await umbracoUi.stylesheet.clickCreateButton();
  await umbracoUi.stylesheet.clickNewStylesheetButton();
  await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
  await umbracoUi.stylesheet.enterStylesheetContent(stylesheetContent);
  await umbracoUi.stylesheet.clickSaveButton();

  // Assert
  //await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.stylesheet.isErrorNotificationVisible(false);
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
  const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
  expect(stylesheetData.content).toEqual(stylesheetContent);
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName);
});

test.skip('can update a stylesheet', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetContent = '/**umb_name:' + styleName + '*/\n' + styleSelector + ' {\n\t' +  styleStyles + '\n}';
  await umbracoApi.stylesheet.create(stylesheetName, '', '/');
  expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeTruthy();
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  //Act
  await umbracoUi.stylesheet.openStylesheetByNameAtRoot(stylesheetName);
  await umbracoUi.stylesheet.clickSaveButton();

  // Assert
  //await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.stylesheet.isErrorNotificationVisible(false);
  const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
  expect(stylesheetData.content).toEqual(stylesheetContent);
});

test('can delete a stylesheet', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  //Act
  await umbracoUi.stylesheet.reloadStylesheetTree();
  await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetName);
  await umbracoUi.stylesheet.clickDeleteAndConfirmButton();

  // Assert
  //await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.stylesheet.isErrorNotificationVisible(false);
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeFalsy();
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName, false, false);
});

test('can rename a stylesheet', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongStylesheetName = 'WrongStylesheetName.css';
  await umbracoApi.stylesheet.ensureNameNotExists(wrongStylesheetName);
  await umbracoApi.stylesheet.createDefaultStylesheet(wrongStylesheetName);
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  //Act
  await umbracoUi.stylesheet.reloadStylesheetTree();
  await umbracoUi.stylesheet.clickActionsMenuForStylesheet(wrongStylesheetName);
  await umbracoUi.stylesheet.rename(stylesheetName);

  // Assert
  await umbracoUi.stylesheet.isErrorNotificationVisible(false);
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
  expect(await umbracoApi.stylesheet.doesNameExist(wrongStylesheetName)).toBeFalsy();
});

test('cannot create a stylesheet with an empty name', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.stylesheet.clickActionsMenuAtRoot();
  await umbracoUi.stylesheet.clickCreateButton();
  await umbracoUi.stylesheet.clickNewStylesheetButton();
  await umbracoUi.stylesheet.clickSaveButton();

  // Assert
  // TODO: Uncomment this when the front-end is ready. Currently there is no error displays.
  //await umbracoUi.stylesheet.isErrorNotificationVisible();
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeFalsy();
});
