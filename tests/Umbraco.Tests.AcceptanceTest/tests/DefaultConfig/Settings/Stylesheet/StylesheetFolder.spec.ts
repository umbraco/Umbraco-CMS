import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const stylesheetName = 'TestStyleSheetFile.css';
const stylesheetFolderName = 'TestStylesheetFolder';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetFolderName);
});

test('can create a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.stylesheet.clickActionsMenuAtRoot();
  await umbracoUi.stylesheet.createFolder(stylesheetFolderName);

  // Assert
  await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetFolderName)).toBeTruthy();
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetFolderName);
});

test('can delete a folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.stylesheet.createFolder(stylesheetFolderName, '');
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.stylesheet.reloadStylesheetTree();
  await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetFolderName);
  await umbracoUi.stylesheet.deleteFolder();

  // Assert
  await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  expect(await umbracoApi.stylesheet.doesFolderExist(stylesheetFolderName)).toBeFalsy();
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetFolderName, false, false);
});

test('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
  const childFolderName = 'ChildFolderName';
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.stylesheet.reloadStylesheetTree();
  await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetFolderName);
  await umbracoUi.stylesheet.createFolder(childFolderName);

  //Assert
  await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.stylesheet.doesNameExist(childFolderName)).toBeTruthy();
  const styleChildren = await umbracoApi.stylesheet.getChildren('/' + stylesheetFolderName);
  expect(styleChildren[0].path).toBe('/' + stylesheetFolderName + '/' + childFolderName);
  await umbracoUi.stylesheet.clickCaretButtonForName(stylesheetFolderName);
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(childFolderName, true, false);
});

test('can create a folder in a folder in a folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildFolderName';
  const childOfChildFolderName = 'ChildOfChildFolderName';
  await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
  await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderName);
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.stylesheet.reloadStylesheetTree();
  await umbracoUi.stylesheet.clickCaretButtonForName(stylesheetFolderName);
  await umbracoUi.stylesheet.clickActionsMenuForStylesheet(childFolderName);
  await umbracoUi.stylesheet.createFolder(childOfChildFolderName);

  // Assert
  await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.stylesheet.doesNameExist(childOfChildFolderName)).toBeTruthy();
  const styleChildren = await umbracoApi.stylesheet.getChildren('/' + stylesheetFolderName + '/' + childFolderName);
  expect(styleChildren[0].path).toBe('/' + stylesheetFolderName + '/' + childFolderName + '/' + childOfChildFolderName);
  await umbracoUi.stylesheet.clickCaretButtonForName(childFolderName);
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(childOfChildFolderName, true, false);
});

test('can create a stylesheet in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
  const stylesheetContent = 'TestContent';
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  //Act
  await umbracoUi.stylesheet.reloadStylesheetTree();
  await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetFolderName);
  await umbracoUi.stylesheet.clickCreateButton();
  await umbracoUi.stylesheet.clickNewStylesheetButton();
  await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
  await umbracoUi.stylesheet.enterStylesheetContent(stylesheetContent);
  await umbracoUi.stylesheet.clickSaveButton();

  // Assert
  await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
  const stylesheetChildren = await umbracoApi.stylesheet.getChildren('/' + stylesheetFolderName);
  expect(stylesheetChildren[0].path).toBe('/' + stylesheetFolderName + '/' + stylesheetName);
  const stylesheetData = await umbracoApi.stylesheet.get(stylesheetChildren[0].path);
  expect(stylesheetData.content).toBe(stylesheetContent);
  await umbracoUi.stylesheet.clickCaretButtonForName(stylesheetFolderName);
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName, true, false);
});

test('can create a stylesheet in a folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildFolderName';
  await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
  await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderName);
  const stylesheetContent = 'TestContent';
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  //Act
  await umbracoUi.stylesheet.reloadStylesheetTree();
  await umbracoUi.stylesheet.clickCaretButtonForName(stylesheetFolderName);
  await umbracoUi.stylesheet.clickActionsMenuForStylesheet(childFolderName);
  await umbracoUi.stylesheet.clickCreateButton();
  await umbracoUi.stylesheet.clickNewStylesheetButton();
  await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
  await umbracoUi.stylesheet.enterStylesheetContent(stylesheetContent);
  await umbracoUi.stylesheet.clickSaveButton();

  // Assert
  await umbracoUi.stylesheet.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
  const stylesheetChildren = await umbracoApi.stylesheet.getChildren('/' + stylesheetFolderName + '/' + childFolderName);
  expect(stylesheetChildren[0].path).toBe('/' + stylesheetFolderName + '/' + childFolderName + '/' + stylesheetName);
  const stylesheetData = await umbracoApi.stylesheet.get(stylesheetChildren[0].path);
  expect(stylesheetData.content).toBe(stylesheetContent);
  await umbracoUi.stylesheet.clickCaretButtonForName(childFolderName);
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName, true, false);
});

test('cannot delete non-empty folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildFolderName';
  await umbracoApi.stylesheet.createFolder(stylesheetFolderName);
  await umbracoApi.stylesheet.createFolder(childFolderName, stylesheetFolderName);
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.stylesheet.clickRootFolderCaretButton();
  await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetFolderName);
  await umbracoUi.stylesheet.deleteFolder();

  //Assert
  await umbracoUi.stylesheet.doesErrorNotificationHaveText(NotificationConstantHelper.error.notEmpty);
});
