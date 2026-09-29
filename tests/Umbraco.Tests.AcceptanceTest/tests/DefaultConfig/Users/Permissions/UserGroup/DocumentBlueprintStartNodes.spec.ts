import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const testUser = ConstantHelper.testUserCredentials;

const userGroupName = 'TestUserGroup';
let userGroupId = null;

let firstFolderId = null;
const firstFolderName = 'FirstBlueprintFolder';
const secondFolderName = 'SecondBlueprintFolder';

const documentTypeName = 'DocumentTypeForBlueprint';
const rootBlueprintName = 'RootBlueprint';
const childBlueprintName = 'ChildBlueprint';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(firstFolderName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(secondFolderName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(rootBlueprintName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  firstFolderId = await umbracoApi.documentBlueprint.createFolder(firstFolderName);
  await umbracoApi.documentBlueprint.createFolder(secondFolderName);
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprint(rootBlueprintName, documentTypeId);
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprintWithParent(childBlueprintName, documentTypeId, firstFolderId);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(firstFolderName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(secondFolderName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(rootBlueprintName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see all document blueprints with root access', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDocumentBlueprintRootAccess(userGroupName);
  await umbracoApi.user.setUserPermissionsForDocumentBlueprint(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(firstFolderName, true);
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(secondFolderName, true, false);
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(rootBlueprintName, true, false);
});

test('can only see the document blueprint start node of the user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDocumentBlueprintStartNode(userGroupName, firstFolderId);
  await umbracoApi.user.setUserPermissionsForDocumentBlueprint(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(firstFolderName, true);
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(secondFolderName, false, false);
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(rootBlueprintName, false, false);
  // Everything below the start node stays reachable.
  await umbracoUi.documentBlueprint.clickCaretButtonForName(firstFolderName);
  await umbracoUi.documentBlueprint.isChildDocumentBlueprintInTreeVisible(firstFolderName, childBlueprintName);
});

test('user document blueprint start node replaces the user group root access', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDocumentBlueprintRootAccess(userGroupName);
  await umbracoApi.user.setUserPermissionsForDocumentBlueprint(testUser.name, testUser.email, testUser.password, userGroupId, [firstFolderId]);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(firstFolderName, true);
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(secondFolderName, false, false);
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(rootBlueprintName, false, false);
  await umbracoUi.documentBlueprint.clickCaretButtonForName(firstFolderName);
  await umbracoUi.documentBlueprint.isChildDocumentBlueprintInTreeVisible(firstFolderName, childBlueprintName);
});

test('cannot see the document blueprint menu without document blueprint access', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createSimpleUserGroupWithLibrarySection(userGroupName);
  await umbracoApi.user.setUserPermissionsForDocumentBlueprint(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.user.goToSection(ConstantHelper.sections.library, false);

  // Assert
  await umbracoUi.documentBlueprint.isDocumentBlueprintSidebarHeaderVisible(false);
});
