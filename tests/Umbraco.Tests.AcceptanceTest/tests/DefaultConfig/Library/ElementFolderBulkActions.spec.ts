import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
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
  // Empty the recycle bin before deleting the folder: bulk-trashed children hold restore relations
  // referencing the folder, so deleting the folder first hits the element-container FK bug (#23387).
  await umbracoApi.element.emptyRecycleBin();
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
  await umbracoApi.element.ensureNameNotExists(firstElementName);
  await umbracoApi.element.ensureNameNotExists(secondElementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can bulk publish elements in a folder', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.library.selectElementWithNameInElementCollectionView(firstElementName);
  await umbracoUi.library.selectElementWithNameInElementCollectionView(secondElementName);
  await umbracoUi.library.clickPublishSelectedListItems();
  await umbracoUi.library.clickConfirmToPublishButtonAndWaitForElementToBePublished();

  // Assert
  expect(await umbracoApi.element.isElementPublished(firstElementId)).toBeTruthy();
  expect(await umbracoApi.element.isElementPublished(secondElementId)).toBeTruthy();
  // Verify audit trail
  const currentUser = await umbracoApi.user.getCurrentUser();
  await umbracoUi.library.clickCaretButtonForElementName(elementFolderName);
  await umbracoUi.library.goToElementWithName(firstElementName);
  await umbracoUi.library.clickInfoTab();
  await umbracoUi.library.doesHistoryItemHaveTag(ConstantHelper.auditTrailTypes.publish);
  await umbracoUi.library.doesHistoryItemHaveDescription(ConstantHelper.auditTrailMessages.elementSavedAndPublished);
  await umbracoUi.library.doesHistoryItemHaveUsername(currentUser.name);
  await umbracoUi.library.goToElementWithName(secondElementName);
  await umbracoUi.library.clickInfoTab();
  await umbracoUi.library.doesHistoryItemHaveTag(ConstantHelper.auditTrailTypes.publish);
  await umbracoUi.library.doesHistoryItemHaveDescription(ConstantHelper.auditTrailMessages.elementSavedAndPublished);
  await umbracoUi.library.doesHistoryItemHaveUsername(currentUser.name);
});

test('can bulk unpublish elements in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.publish(firstElementId);
  await umbracoApi.element.publish(secondElementId);

  // Act
  await umbracoUi.library.selectElementWithNameInElementCollectionView(firstElementName);
  await umbracoUi.library.selectElementWithNameInElementCollectionView(secondElementName);
  await umbracoUi.library.clickUnpublishSelectedListItems();
  await umbracoUi.library.clickConfirmToUnpublishButtonAndWaitForElementToBeUnpublished();

  // Assert
  // The confirm waiter resolves on the first element's unpublish response; the second can still be
  // settling, so poll each state rather than reading once.
  await expect.poll(() => umbracoApi.element.isElementPublished(firstElementId)).toBeFalsy();
  await expect.poll(() => umbracoApi.element.isElementPublished(secondElementId)).toBeFalsy();
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

test('can bulk trash elements in a folder', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.library.selectElementWithNameInElementCollectionView(firstElementName);
  await umbracoUi.library.selectElementWithNameInElementCollectionView(secondElementName);
  await umbracoUi.library.clickTrashSelectedListItems();
  await umbracoUi.library.clickConfirmTrashButtonAndWaitForElementToBeTrashed();

  // Assert
  // The confirm waiter resolves on the first element's trash response; the second can still be
  // settling, so poll each state rather than reading once.
  await expect.poll(() => umbracoApi.element.doesNameExist(firstElementName)).toBeFalsy();
  await expect.poll(() => umbracoApi.element.doesNameExist(secondElementName)).toBeFalsy();
  await expect.poll(() => umbracoApi.element.getChildren(folderId).then(c => c.length)).toBe(0);
});
