import {expect} from '@playwright/test';
import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

const userGroupName = 'TestPropertyValuePermission';
let userGroupId = null;

const firstDocumentName = 'FirstTestDocument';
const secondDocumentName = 'SecondTestDocument';
const documentTypeName = 'TestDocumentType';
const firstPropertyName = ['Textstring', 'text-box'];
const secondPropertyName = ['True/false', 'toggle'];
let documentTypeId = null;
let firstDocumentId = null;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(firstDocumentName);
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  const firstPropertyData = await umbracoApi.dataType.getByName(firstPropertyName[0]);
  const secondPropertyData = await umbracoApi.dataType.getByName(secondPropertyName[0]);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTwoPropertyEditors(documentTypeName, firstPropertyName[0], firstPropertyData.id, secondPropertyName[0], secondPropertyData.id);
  firstDocumentId = await umbracoApi.document.createDefaultDocument(firstDocumentName, documentTypeId);
  await umbracoApi.document.createDefaultDocument(secondDocumentName, documentTypeId);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(firstDocumentName);
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

// Skip this test due to this issue: https://github.com/umbraco/Umbraco-CMS/issues/20505
test.skip('can only see property values for specific document with read UI enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPermissionsForSpecificDocumentAndTwoPropertyValues(userGroupName, firstDocumentId, documentTypeId, firstPropertyName[0], true, false, secondPropertyName[0], true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Assert
  await umbracoUi.content.goToContentWithName(firstDocumentName);
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly(firstPropertyName[1]);
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly(secondPropertyName[1]);
  await umbracoUi.content.goToContentWithName(secondDocumentName);
  await umbracoUi.content.doesDocumentWorkspaceHaveText('Not found');
});

test('cannot see specific property value without UI read permission enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithPermissionsForSpecificDocumentAndTwoPropertyValues(userGroupName, firstDocumentId, documentTypeId, firstPropertyName[0], false, false, secondPropertyName[0], false, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
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
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
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
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
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
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
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
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(firstDocumentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameVisible(firstPropertyName[1], false);
  await umbracoUi.content.isPropertyEditorUiWithNameVisible(secondPropertyName[1], false);
});
