import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
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

// Currently this test fails due to the issue: an 500 error is thrown when trying to empty the recycle bin 
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
  await umbracoUi.library.clickConfirmEmptyRecycleBinButtonAndWaitForRecycleBinToBeEmptied();

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

test('can see trashed element folder in recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementFolderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  await umbracoApi.element.moveToRecycleBin(elementFolderId, true);

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Assert
  await umbracoUi.library.isItemVisibleInRecycleBin(elementFolderName);
});

// Currently this test fails due to the issue: an 500 error is thrown when performance this action
test('can delete element from recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.moveToRecycleBin(elementId);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeTruthy();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName);

  // Act
  await umbracoUi.library.clickDeleteButtonForTrashedElememtWithName(elementName);
  await umbracoUi.library.clickConfirmToDeleteButtonAndWaitForElementToBeDeleted();

  // Assert
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeFalsy();
});

// Currently this test fails due to the issue: an 500 error is thrown when performance this action
test('can delete element folder from recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementFolderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  await umbracoApi.element.moveToRecycleBin(elementFolderId, true);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeTruthy();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementFolderName);

  // Act
  await umbracoUi.library.clickDeleteButtonForTrashedElememtWithName(elementFolderName);
  await umbracoUi.library.clickConfirmToDeleteButtonAndWaitForElementToBeDeleted();

  // Assert
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeFalsy();
});

// Currently this test fails due to the issue: an 500 error is thrown when performance this action
test('can delete element folder with children from recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  await umbracoApi.element.createDefaultElementWithParent(elementName, elementTypeId, folderId);
  await umbracoApi.element.moveToRecycleBin(folderId, true);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeTruthy();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementFolderName);

  // Act
  await umbracoUi.library.clickDeleteButtonForTrashedElememtWithName(elementFolderName);
  await umbracoUi.library.clickConfirmToDeleteButtonAndWaitForElementToBeDeleted();

  // Assert
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeFalsy();
});

// Currently this test fails due to the issue: an 500 error is thrown when performance this action
test('can delete child element from recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  const elementId = await umbracoApi.element.createDefaultElementWithParent(elementName, elementTypeId, folderId);
  await umbracoApi.element.moveToRecycleBin(elementId);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeTruthy();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName);

  // Act
  await umbracoUi.library.clickDeleteButtonForTrashedElememtWithName(elementName);
  await umbracoUi.library.clickConfirmToDeleteButtonAndWaitForElementToBeDeleted();

  // Assert
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeFalsy();
  // Verify the parent folder still exists
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeTruthy();
});

test('can restore element from recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.moveToRecycleBin(elementId);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeTruthy();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName);
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickRestoreActionMenuOption();
  await umbracoUi.library.isRestoreFromRecycleBinMessageVisible(elementName, 'Root');
  await umbracoUi.library.clickRestoreButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.restored);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementName);
});

test('can restore element folder from recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementFolderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  await umbracoApi.element.moveToRecycleBin(elementFolderId, true);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeTruthy();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementFolderName);
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickRestoreActionMenuOption();
  await umbracoUi.library.isRestoreFromRecycleBinMessageVisible(elementFolderName, 'Root');
  await umbracoUi.library.clickRestoreButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.restored);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementFolderName);
});

// Currently this test fails due to the wrong restore message being shown
test('can restore child element from recycle bin back to its parent folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  const elementId = await umbracoApi.element.createDefaultElementWithParent(elementName, elementTypeId, folderId);
  await umbracoApi.element.moveToRecycleBin(elementId);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeTruthy();
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeTruthy();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName);
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickRestoreActionMenuOption();
  await umbracoUi.library.isRestoreFromRecycleBinMessageVisible(elementName, elementFolderName);
  await umbracoUi.library.clickRestoreButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.restored);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  // Verify the element is restored back under its parent folder
  const children = await umbracoApi.element.getChildren(folderId);
  expect(children.some(child => child.name === elementName)).toBeTruthy();
});

test('can restore element folder with children from recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  await umbracoApi.element.createDefaultElementWithParent(elementName, elementTypeId, folderId);
  await umbracoApi.element.moveToRecycleBin(folderId, true);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeTruthy();

  // Act
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementFolderName);
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickRestoreActionMenuOption();
  await umbracoUi.library.isRestoreFromRecycleBinMessageVisible(elementFolderName, 'Root');
  await umbracoUi.library.clickRestoreButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.restored);
  expect(await umbracoApi.element.doesItemExistInRecycleBin(elementFolderName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(elementFolderName)).toBeTruthy();
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementFolderName);
});

