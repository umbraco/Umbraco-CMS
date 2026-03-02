import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const elementFolderName = 'TestElementFolder';
const firstElementName = 'FirstTestElement';
const secondElementName = 'SecondTestElement';
const elementTypeName = 'TestElementTypeForBulk';
const dataTypeName = 'Textstring';
let elementTypeId = '';
let folderId = '';
let firstElementId = '';
let secondElementId = '';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id);
  folderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  firstElementId = await umbracoApi.element.createDefaultElementWithParent(firstElementName, elementTypeId, folderId);
  secondElementId = await umbracoApi.element.createDefaultElementWithParent(secondElementName, elementTypeId, folderId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementFolderName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
  await umbracoApi.element.ensureNameNotExists(firstElementName);
  await umbracoApi.element.ensureNameNotExists(secondElementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can bulk publish elements in a folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.library.selectElementWithNameInElementCollectionView(firstElementName);
  await umbracoUi.library.selectElementWithNameInElementCollectionView(secondElementName);
  await umbracoUi.library.clickPublishSelectedListItems();
  await umbracoUi.library.clickConfirmToPublishButtonAndWaitForElementToBePublished();

  // Assert
  expect(await umbracoApi.element.isElementPublished(firstElementId)).toBeTruthy();
  expect(await umbracoApi.element.isElementPublished(secondElementId)).toBeTruthy();
});

test('can bulk unpublish elements in a folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.publish(firstElementId);
  await umbracoApi.element.publish(secondElementId);

  // Act
  await umbracoUi.library.selectElementWithNameInElementCollectionView(firstElementName);
  await umbracoUi.library.selectElementWithNameInElementCollectionView(secondElementName);
  await umbracoUi.library.clickUnpublishSelectedListItems();
  await umbracoUi.library.clickConfirmToUnpublishButtonAndWaitForElementToBeUnpublished();

  // Assert
  expect(await umbracoApi.element.isElementPublished(firstElementId)).toBeFalsy();
  expect(await umbracoApi.element.isElementPublished(secondElementId)).toBeFalsy();
});

test('can bulk move elements to another folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const targetFolderName = 'TargetElementFolder';
  await umbracoApi.element.ensureNameNotExists(targetFolderName);
  const targetFolderId = await umbracoApi.element.createDefaultElementFolder(targetFolderName);

  // Act
  await umbracoUi.library.selectElementWithNameInElementCollectionView(firstElementName);
  await umbracoUi.library.selectElementWithNameInElementCollectionView(secondElementName);
  await umbracoUi.library.clickMoveToSelectedListItems();
  await umbracoUi.library.moveToElementWithName(['Elements'], targetFolderName);

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  const targetFolderChildren = await umbracoApi.element.getChildren(targetFolderId);
  expect(targetFolderChildren.length).toBe(2);
  const childNames = targetFolderChildren.map((child: any) => child.name);
  expect(childNames).toContain(firstElementName);
  expect(childNames).toContain(secondElementName);

  // Clean
  await umbracoApi.element.ensureNameNotExists(targetFolderName);
});

test('can bulk trash elements in a folder', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.library.selectElementWithNameInElementCollectionView(firstElementName);
  await umbracoUi.library.selectElementWithNameInElementCollectionView(secondElementName);
  await umbracoUi.library.clickTrashSelectedListItems();
  await umbracoUi.library.clickConfirmTrashButtonAndWaitForElementToBeTrashed();

  // Assert
  expect(await umbracoApi.element.doesNameExist(firstElementName)).toBeFalsy();
  expect(await umbracoApi.element.doesNameExist(secondElementName)).toBeFalsy();
  const folderChildren = await umbracoApi.element.getChildren(folderId);
  expect(folderChildren.length).toBe(0);
});
