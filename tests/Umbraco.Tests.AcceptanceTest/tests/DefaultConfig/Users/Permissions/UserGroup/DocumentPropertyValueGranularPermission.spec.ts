import {expect} from '@playwright/test';
import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const testUser = ConstantHelper.testUserCredentials;

const userGroupName = 'TestPropertyValuePermission';
let userGroupId = null;

const firstDocumentName = 'FirstTestDocument';
const secondDocumentName = 'SecondTestDocument';
const documentTypeName = 'TestDocumentType';
const firstPropertyName = ['Textstring', 'text-box'];
const secondPropertyName = ['True/false', 'toggle'];
let documentTypeId = null;
let firstDocumentId = null;
let secondDocumentId = null;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(firstDocumentName);
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  const firstPropertyData = await umbracoApi.dataType.getByName(firstPropertyName[0]);
  const secondPropertyData = await umbracoApi.dataType.getByName(secondPropertyName[0]);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTwoPropertyEditors(documentTypeName, firstPropertyName[0], firstPropertyData.id, secondPropertyName[0], secondPropertyData.id);
  firstDocumentId = await umbracoApi.document.createDefaultDocument(firstDocumentName, documentTypeId);
  secondDocumentId = await umbracoApi.document.createDefaultDocument(secondDocumentName, documentTypeId);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser();
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(firstDocumentName);
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

// Blocked only by the shared forbidden-workspace gap (https://github.com/umbraco/Umbraco-CMS/issues/20505):
// everything up to the last two lines passes, so the granular read-UI permission itself works. Deep-linking to
// the document outside the permission renders an empty umb-document-workspace-editor instead of Access denied.
// The navigation defect is fixed here - the second document is absent from the tree, so it must be deep-linked.
test.skip('can only see property values for specific document with read UI enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPermissionsForSpecificDocumentAndTwoPropertyValues(userGroupName, firstDocumentId, documentTypeId, firstPropertyName[0], true, false, secondPropertyName[0], true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.goToContentWithName(firstDocumentName);
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly(firstPropertyName[1]);
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly(secondPropertyName[1]);
  // The second document is outside the granular permission, so it is absent from the tree and must be deep-linked.
  await umbracoUi.content.goToWorkspacePath(`/workspace/document/edit/${secondDocumentId}`);
  await umbracoUi.content.doesDocumentWorkspaceHaveText('Not found');
});

test('cannot see specific property value without UI read permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPermissionsForSpecificDocumentAndTwoPropertyValues(userGroupName, firstDocumentId, documentTypeId, firstPropertyName[0], false, false, secondPropertyName[0], false, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(firstDocumentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameVisible(firstPropertyName[1], false);
  await umbracoUi.content.isPropertyEditorUiWithNameVisible(secondPropertyName[1], false);
});

test('can see specific property values with UI read permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPermissionsForSpecificDocumentAndTwoPropertyValues(userGroupName, firstDocumentId, documentTypeId, firstPropertyName[0], true, false, secondPropertyName[0], true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(firstDocumentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly(firstPropertyName[1]);
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly(secondPropertyName[1]);
});

test('can see property with UI read enabled but not another property with UI read disabled in the same document', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPermissionsForSpecificDocumentAndTwoPropertyValues(userGroupName, firstDocumentId, documentTypeId, firstPropertyName[0], true, false, secondPropertyName[0], false, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(firstDocumentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly(firstPropertyName[1]);
  await umbracoUi.content.isPropertyEditorUiWithNameVisible(secondPropertyName[1], false);
});

test('can edit specific property values with UI read and write permission enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is test text';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPermissionsForSpecificDocumentAndTwoPropertyValues(userGroupName, firstDocumentId, documentTypeId, firstPropertyName[0], true, true, secondPropertyName[0], true, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(firstDocumentName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickToggleButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const firstDocumentData = await umbracoApi.document.getByName(firstDocumentName);
  expect(firstDocumentData.values[0].alias).toEqual(AliasHelper.toAlias(firstPropertyName[0]));
  expect(firstDocumentData.values[0].value).toEqual(inputText);
  expect(firstDocumentData.values[1].alias).toEqual(AliasHelper.toAlias(secondPropertyName[0]).replace('/', ''));
  expect(firstDocumentData.values[1].value).toEqual(true);
});

test('cannot see specific property values with UI write permission enabled and UI read permission disabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPermissionsForSpecificDocumentAndTwoPropertyValues(userGroupName, firstDocumentId, documentTypeId, firstPropertyName[0], false, true, secondPropertyName[0], false, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(firstDocumentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameVisible(firstPropertyName[1], false);
  await umbracoUi.content.isPropertyEditorUiWithNameVisible(secondPropertyName[1], false);
});
