import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const elementFolderName = 'TestElementFolder';
const elementName = 'TestElementInFolder';
const elementTypeName = 'TestElementTypeForFolder';
const dataTypeName = 'Textstring';
let elementTypeId = '';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can create an empty element folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementFolderButton();
  await umbracoUi.library.enterFolderName(elementFolderName);
  await umbracoUi.library.clickCreateFolderButtonAndWaitForElementFolderToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementFolderName);
});

test('can trash an element folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createDefaultElementFolder(elementFolderName);

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickTrashActionMenuOption();
  await umbracoUi.library.clickConfirmTrashButtonAndWaitForElementFolderToBeTrashed();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeFalsy();
  await umbracoUi.library.isElementInTreeVisible(elementFolderName, false);
});

test('can rename an element folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const oldFolderName = 'OldFolderName';
  await umbracoApi.element.ensureNameNotExists(oldFolderName);
  await umbracoApi.element.createDefaultElementFolder(oldFolderName);

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.clickActionsMenuForElement(oldFolderName);
  await umbracoUi.library.clickRenameActionMenuOption();
  await umbracoUi.library.enterFolderName(elementFolderName);
  await umbracoUi.library.clickConfirmRenameFolderButtonAndWaitForElementFolderToBeRenamed();

  // Assert
  expect(await umbracoApi.element.doesNameExist(oldFolderName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(oldFolderName, false);
  await umbracoUi.library.isElementInTreeVisible(elementFolderName);
});

test('can create an element folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildElementFolder';
  await umbracoApi.element.ensureNameNotExists(childFolderName);
  const parentFolderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementFolderButton();
  await umbracoUi.library.enterFolderName(childFolderName);
  await umbracoUi.library.clickCreateFolderButtonAndWaitForElementFolderToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(childFolderName)).toBeTruthy();
  const parentFolder = await umbracoApi.element.getChildren(parentFolderId);
  expect(parentFolder[0].name).toBe(childFolderName);

  // Clean
  await umbracoApi.element.ensureNameNotExists(childFolderName);
});

test('can create a folder in a folder in a folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const grandParentFolderName = 'GrandParentElementFolder';
  const parentFolderName = 'ParentElementFolder';
  await umbracoApi.element.ensureNameNotExists(grandParentFolderName);
  await umbracoApi.element.ensureNameNotExists(parentFolderName);
  const grandParentFolderId = await umbracoApi.element.createDefaultElementFolder(grandParentFolderName);
  const parentFolderId = await umbracoApi.element.createFolder(parentFolderName, grandParentFolderId);

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.openElementCaretButtonForName(grandParentFolderName);
  await umbracoUi.library.clickActionsMenuForElement(parentFolderName);
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementFolderButton();
  await umbracoUi.library.enterFolderName(elementFolderName);
  await umbracoUi.library.clickCreateFolderButtonAndWaitForElementFolderToBeCreated();

  // Assert
  await umbracoUi.library.clickActionsMenuForElement(parentFolderName);
  await umbracoUi.library.clickReloadChildrenActionMenuOption();
  await umbracoUi.library.openElementCaretButtonForName(parentFolderName);
  await umbracoUi.library.isChildElementInTreeVisible(parentFolderName, elementFolderName);
  const grandParentChildren = await umbracoApi.element.getChildren(grandParentFolderId);
  expect(grandParentChildren[0].name).toBe(parentFolderName);
  const parentChildren = await umbracoApi.element.getChildren(parentFolderId);
  expect(parentChildren[0].name).toBe(elementFolderName);

  // Clean
  await umbracoApi.element.ensureNameNotExists(grandParentFolderName);
  await umbracoApi.element.ensureNameNotExists(parentFolderName);
});

test('can create and publish an element in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(elementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(elementName);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  const folderChildren = await umbracoApi.element.getChildren(folderId);
  expect(folderChildren[0].name).toBe(elementName);
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe('Published');
});

test('can create an element in a nested folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentFolderName = 'ParentElementFolder';
  await umbracoApi.element.ensureNameNotExists(parentFolderName);
  const parentFolderId = await umbracoApi.element.createDefaultElementFolder(parentFolderName);
  const childFolderId = await umbracoApi.element.createFolder(elementFolderName, parentFolderId);

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.openElementCaretButtonForName(parentFolderName);
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(elementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(elementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  const folderChildren = await umbracoApi.element.getChildren(childFolderId);
  expect(folderChildren[0].name).toBe(elementName);

  // Clean
  await umbracoApi.element.ensureNameNotExists(parentFolderName);
});

