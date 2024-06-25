import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const partialViewName = 'TestPartialView';
const partialViewFileName = partialViewName + '.cshtml';
const folderName = 'TestFolder';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.partialView.ensureNameNotExists(folderName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.partialView.ensureNameNotExists(folderName);
});

test('can create a folder', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.partialView.clickActionsMenuAtRoot();
  await umbracoUi.partialView.createFolder(folderName);

  // Assert
  await umbracoUi.partialView.isSuccessNotificationVisible();
  expect(await umbracoApi.partialView.doesFolderExist(folderName)).toBeTruthy();
  // Verify the partial view folder is displayed under the Partial Views section
  await umbracoUi.partialView.clickRootFolderCaretButton();
  await umbracoUi.partialView.isPartialViewRootTreeItemVisibile(folderName);
});

test('can delete a folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  //Arrange
  await umbracoApi.partialView.createFolder(folderName);
  expect(await umbracoApi.partialView.doesFolderExist(folderName)).toBeTruthy();

  // Act
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.partialView.reloadPartialViewTree();
  await umbracoUi.partialView.clickActionsMenuForPartialView(folderName);
  await umbracoUi.partialView.deleteFolder();

  // Assert
  await umbracoUi.partialView.isSuccessNotificationVisible();
  expect(await umbracoApi.partialView.doesFolderExist(folderName)).toBeFalsy();
  // Verify the partial view folder is NOT displayed under the Partial Views section
  await umbracoUi.partialView.clickRootFolderCaretButton();
  await umbracoUi.partialView.isPartialViewRootTreeItemVisibile(folderName, false, false);
});

test('can place a partial view into folder', async ({umbracoApi, umbracoUi}) => {
  //Arrange
  await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
  const folderPath = await umbracoApi.partialView.createFolder(folderName);
  expect(await umbracoApi.partialView.doesFolderExist(folderName)).toBeTruthy();

  // Act
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.partialView.reloadPartialViewTree();
  await umbracoUi.partialView.clickActionsMenuForPartialView(folderName);
  await umbracoUi.partialView.clickCreateButton();
  await umbracoUi.partialView.clickNewEmptyPartialViewButton();
  await umbracoUi.partialView.enterPartialViewName(partialViewName);
  await umbracoUi.partialView.clickSaveButton();

  // Assert
  await umbracoUi.partialView.isSuccessNotificationVisible();
  const childrenData = await umbracoApi.partialView.getChildren(folderPath);
  expect(childrenData[0].name).toEqual(partialViewFileName);
  // Verify the partial view is displayed in the folder under the Partial Views section
  await umbracoUi.partialView.isPartialViewRootTreeItemVisibile(partialViewFileName, false, false);
  await umbracoUi.partialView.clickCaretButtonForName(folderName);
  await umbracoUi.partialView.isPartialViewRootTreeItemVisibile(partialViewFileName, true, false);
});

test('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.partialView.createFolder(folderName);
  const childFolderName = 'childFolderName';

  // Act
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.partialView.reloadPartialViewTree();
  await umbracoUi.partialView.clickActionsMenuForPartialView(folderName);
  await umbracoUi.partialView.createFolder(childFolderName);

  // Assert
  await umbracoUi.partialView.isSuccessNotificationVisible();
  expect(await umbracoApi.partialView.doesNameExist(childFolderName)).toBeTruthy();
  const partialViewChildren = await umbracoApi.partialView.getChildren('/' + folderName);
  expect(partialViewChildren[0].path).toBe('/' + folderName + '/' + childFolderName);
  await umbracoUi.partialView.clickCaretButtonForName(folderName);
  await umbracoUi.partialView.isPartialViewRootTreeItemVisibile(childFolderName, true, false);
});

test('can create a folder in a folder in a folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildFolderName';
  const childOfChildFolderName = 'ChildOfChildFolderName';
  await umbracoApi.partialView.createFolder(folderName);
  await umbracoApi.partialView.createFolder(childFolderName, folderName);

  // Act
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.partialView.reloadPartialViewTree();
  await umbracoUi.partialView.clickCaretButtonForName(folderName);
  await umbracoUi.partialView.clickActionsMenuForPartialView(childFolderName);
  await umbracoUi.partialView.createFolder(childOfChildFolderName);

  // Assert
  await umbracoUi.partialView.isSuccessNotificationVisible();
  expect(await umbracoApi.partialView.doesNameExist(childOfChildFolderName)).toBeTruthy();
  const partialViewChildren = await umbracoApi.partialView.getChildren('/' + folderName + '/' + childFolderName);
  expect(partialViewChildren[0].path).toBe('/' + folderName + '/' + childFolderName + '/' + childOfChildFolderName);
  await umbracoUi.partialView.clickCaretButtonForName(childFolderName);
  await umbracoUi.partialView.isPartialViewRootTreeItemVisibile(childOfChildFolderName, true, false);
});
