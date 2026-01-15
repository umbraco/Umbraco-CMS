import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const stylesheetName = 'TestStyleSheetFile.css';
const stylesheetContent = 'TestContent';

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
  await umbracoUi.stylesheet.clickCreateActionMenuOption();
  await umbracoUi.stylesheet.clickNewStylesheetButton();
  await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
  await umbracoUi.stylesheet.clickSaveButtonAndWaitForStylesheetToBeCreated();

  // Assert
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName);
});

test('can create a stylesheet with content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  //Act
  await umbracoUi.stylesheet.clickActionsMenuAtRoot();
  await umbracoUi.stylesheet.clickCreateActionMenuOption();
  await umbracoUi.stylesheet.clickNewStylesheetButton();
  await umbracoUi.stylesheet.enterStylesheetName(stylesheetName);
  await umbracoUi.stylesheet.enterStylesheetContent(stylesheetContent);
  await umbracoUi.stylesheet.clickSaveButtonAndWaitForStylesheetToBeCreated();

  // Assert
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
  const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
  expect(stylesheetData.content).toEqual(stylesheetContent);
  await umbracoUi.stylesheet.isStylesheetRootTreeItemVisible(stylesheetName);
});

test('can update a stylesheet', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedContent = 'UpdatedTestContent';
  await umbracoApi.stylesheet.create(stylesheetName, stylesheetContent, '/');
  expect(await umbracoApi.stylesheet.doesExist(stylesheetName)).toBeTruthy();
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  //Act
  await umbracoUi.stylesheet.openStylesheetByNameAtRoot(stylesheetName);
  await umbracoUi.stylesheet.enterStylesheetContent(updatedContent);
  await umbracoUi.stylesheet.clickSaveButtonAndWaitForStylesheetToBeUpdated();

  // Assert
  const stylesheetData = await umbracoApi.stylesheet.getByName(stylesheetName);
  expect(stylesheetData.content).toEqual(updatedContent);
});

test('can delete a stylesheet', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  //Act
  await umbracoUi.stylesheet.reloadStylesheetTree();
  await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetName);
  await umbracoUi.stylesheet.clickDeleteAndConfirmButtonAndWaitForStylesheetToBeDeleted();

  // Assert
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
  await umbracoUi.stylesheet.renameAndWaitForStylesheetToBeRenamed(stylesheetName);

  // Assert
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeTruthy();
  expect(await umbracoApi.stylesheet.doesNameExist(wrongStylesheetName)).toBeFalsy();
});

test('cannot create a stylesheet with an empty name', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.stylesheet.clickActionsMenuAtRoot();
  await umbracoUi.stylesheet.clickCreateActionMenuOption();
  await umbracoUi.stylesheet.clickNewStylesheetButton();
  await umbracoUi.stylesheet.clickSaveButton();

  // Assert
  await umbracoUi.stylesheet.isFailedStateButtonVisible();
  expect(await umbracoApi.stylesheet.doesNameExist(stylesheetName)).toBeFalsy();
});
