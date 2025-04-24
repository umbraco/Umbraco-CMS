import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Skip all tests as this feature is removed
const relationTypeName = 'Test Relation Type';
const objectTypeName = 'Document';
let relationTypeId = '';
let objectTypeId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.relationType.ensureNameNotExists(relationTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.relationType.goToSettingsTreeItem('Relation Types');
  objectTypeId = await umbracoApi.objectTypes.getIdByName(objectTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.relationType.ensureNameNotExists(relationTypeName);
});

test.skip('can create a relation type', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.relationType.clickActionsMenuAtRoot();
  await umbracoUi.relationType.clickCreateButton();
  await umbracoUi.relationType.enterRelationTypeName(relationTypeName);
  await umbracoUi.relationType.selectParentOption(objectTypeName);
  await umbracoUi.relationType.selectChildOption(objectTypeName);
  await umbracoUi.relationType.clickSaveButton();

  // Assert
  //await umbracoUi.relationType.isSuccessNotificationVisible();
  await umbracoUi.relationType.isErrorNotificationVisible(false);
  expect(await umbracoApi.relationType.doesNameExist(relationTypeName)).toBeTruthy();
  // TODO: when frontend is ready, verify the new relation type name is displayed in the Relation Types tree
});

test.skip('can update name of a relation type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongRelationTypeName = 'Updated Relation Type';
  await umbracoApi.relationType.ensureNameNotExists(wrongRelationTypeName);
  relationTypeId = await umbracoApi.relationType.create(wrongRelationTypeName, false, false, objectTypeId, objectTypeId);

  // Act
  await umbracoUi.relationType.openRelationTypeByNameAtRoot(wrongRelationTypeName);
  await umbracoUi.relationType.enterRelationTypeName(relationTypeName);
  await umbracoUi.relationType.clickSaveButton();

  // Assert
  //await umbracoUi.relationType.isSuccessNotificationVisible();
  await umbracoUi.relationType.isErrorNotificationVisible(false);
  const relationTypeData = await umbracoApi.relationType.get(relationTypeId);
  expect(relationTypeData.name).toEqual(relationTypeName);
  expect(await umbracoApi.relationType.doesNameExist(wrongRelationTypeName)).toBeFalsy();
});

test.skip('can update direction value of a relation type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  relationTypeId = await umbracoApi.relationType.create(relationTypeName, false, false, objectTypeId, objectTypeId);

  // Act
  await umbracoUi.relationType.openRelationTypeByNameAtRoot(relationTypeName);
  await umbracoUi.relationType.clickBidirectionalRadioButton();
  await umbracoUi.relationType.clickSaveButton();

  // Assert
  //await umbracoUi.relationType.isSuccessNotificationVisible();
  await umbracoUi.relationType.isErrorNotificationVisible(false);
  const relationTypeData = await umbracoApi.relationType.get(relationTypeId);
  expect(relationTypeData.isBidirectional).toEqual(true);
});

test.skip('can update isDependency value of a relation type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedObjectTypeName = 'Media';
  relationTypeId = await umbracoApi.relationType.create(relationTypeName, false, false, objectTypeId, objectTypeId);

  // Act
  await umbracoUi.relationType.openRelationTypeByNameAtRoot(relationTypeName);
  await umbracoUi.relationType.clickIsDependencyToggle();
  await umbracoUi.relationType.clickSaveButton();

  // Assert
  //await umbracoUi.relationType.isSuccessNotificationVisible();
  await umbracoUi.relationType.isErrorNotificationVisible(false);
  const relationTypeData = await umbracoApi.relationType.get(relationTypeId);
  expect(relationTypeData.isDependency).toEqual(true);
});

test.skip('can delete a relation type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.relationType.create(relationTypeName, false, false, objectTypeId, objectTypeId);

  // Act
  await umbracoUi.relationType.clickRootFolderCaretButton();
  await umbracoUi.relationType.clickActionsMenuForRelationType(relationTypeName);
  await umbracoUi.relationType.clickDeleteButton();
  await umbracoUi.relationType.clickConfirmToDeleteButton();

  // Assert
  //await umbracoUi.relationType.isSuccessNotificationVisible();
  await umbracoUi.relationType.isErrorNotificationVisible(false);
  expect(await umbracoApi.relationType.doesNameExist(relationTypeName)).toBeFalsy();
  // TODO: when frontend is ready, verify the deleted relation type name is NOT displayed in the Relation Types tree
});

test.skip('can show relations of a relation type', async ({umbracoApi, umbracoUi}) => {
  // TODO: implement this later as the frontend is missing now
});
