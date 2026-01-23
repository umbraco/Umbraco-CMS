import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const elementName = 'TestElementForRecycleBin';
const elementFolderName = 'TestElementFolderForRecycleBin';
const elementTypeName = 'TestElementTypeForRecycleBin';
const dataTypeName = 'Textstring';
let elementTypeId = '';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.element.emptyRecycleBin();
});

test('can trash an element', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickTrashActionMenuOption();
  await umbracoUi.library.clickConfirmTrashButtonAndWaitForElementToBeTrashed();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeFalsy();
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementName, false);
});

test('can trash an element folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeTruthy();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickTrashActionMenuOption();
  await umbracoUi.library.clickConfirmTrashButtonAndWaitForElementFolderToBeTrashed();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeFalsy();
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementFolderName, false);
});

test('can trash an element folder with children', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  await umbracoApi.element.createDefaultElementWithParent(elementName, elementTypeId, folderId);
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeTruthy();
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickTrashActionMenuOption();
  await umbracoUi.library.clickConfirmTrashButtonAndWaitForElementFolderToBeTrashed();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeFalsy();
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementFolderName, false);
});

test('can empty recycle bin', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.moveToRecycleBin(elementId);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeTruthy();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickRecycleBinButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.library.clickEmptyRecycleBinButton();
  await umbracoUi.library.clickConfirmEmptyRecycleBinButtonAndWaitForElementRecycleBinToBeEmptied();

  // Assert
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeFalsy();
});

test('can see trashed element in recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.moveToRecycleBin(elementId);

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Assert
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName);
});

test('can delete element permanently from recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.moveToRecycleBin(elementId);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeTruthy();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickDeleteActionMenuOption();
  await umbracoUi.library.clickConfirmToDeleteButton();

  // Assert
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeFalsy();
});

// Unskip once the front-end is ready
test.skip('can restore element from recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.moveToRecycleBin(elementId);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeTruthy();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName);
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickRestoreActionMenuOption();
  await umbracoUi.library.clickRestoreButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.restored);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementName);
});

