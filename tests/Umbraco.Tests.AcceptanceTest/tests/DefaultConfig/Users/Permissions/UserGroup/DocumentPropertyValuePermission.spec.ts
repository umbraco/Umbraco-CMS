import {expect} from '@playwright/test';
import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const testUser = ConstantHelper.testUserCredentials;

const userGroupName = 'TestPropertyValuePermission';
let userGroupId = null;

const documentName = 'TestDocument';
const documentTypeName = 'TestDocumentType';
const dataTypeName = 'Textstring';
const textString = 'This is test textstring';
let documentId = null;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(documentName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  documentId = await umbracoApi.document.createDocumentWithTextContent(documentName, documentTypeId, textString, dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser();
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test('cannot see property values without UI read permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadDocumentPermissionAndReadPropertyValueDocumentPermission(userGroupName, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('text-box', false);
});

test('can see property values with UI read but not UI write permission', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadDocumentPermissionAndReadPropertyValueDocumentPermission(userGroupName, true, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly('text-box');
});

// Product gap (https://github.com/umbraco/Umbraco-CMS/issues/20505): deep-linking to a document the user may not
// read renders an empty workspace. Expected text should be 'Access denied'. Navigation fixed to deep-link.
test.skip('cannot open content without document read permission even with UI read permission', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadDocumentPermissionAndReadPropertyValueDocumentPermission(userGroupName, false, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  // Without document read permission the node is not in the tree at all, so it has to be deep-linked.
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToWorkspacePath(`/workspace/document/edit/${documentId}`);

  // Assert
  await umbracoUi.content.doesDocumentWorkspaceHaveText('Not found');
});

test('cannot edit property values without UI write permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateDocumentPermissionAndWritePropertyValueDocumentPermission(userGroupName, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isDocumentReadOnly(false);
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly('text-box');
});

test('can edit property values with UI write permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedText = 'Updated test text';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateDocumentPermissionAndWritePropertyValueDocumentPermission(userGroupName, true, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.enterTextstring(updatedText);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const documentData = await umbracoApi.document.getByName(documentName);
  expect(documentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(documentData.values[0].value).toEqual(updatedText);
});

test('cannot see property values with only UI write but no UI read permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdateDocumentPermissionAndWritePropertyValueDocumentPermission(userGroupName, true, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('text-box', false);
});
