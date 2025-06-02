import { expect } from '@playwright/test';
import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

const userGroupName = 'TestPropertyValuePermission';
let userGroupId = null;

const documentName = 'TestDocument';
const documentTypeName = 'TestDocumentType';
const dataTypeName = 'Textstring';
const textString = 'This is test textstring';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(documentName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithTextContent(documentName, documentTypeId, textString, dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test('cannot see property values without UI read permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadPermissionAndReadPropertyValuePermission(userGroupName, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('text-box', false);
});

test('can see property values with UI read but not UI write permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadPermissionAndReadPropertyValuePermission(userGroupName, true, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly('text-box');
});

test('cannot open content without document read permission even with UI read permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadPermissionAndReadPropertyValuePermission(userGroupName, false, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.permissionDenied);
});

test('cannot edit property values without UI write permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermissionAndWritePropertyValuePermission(userGroupName, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isDocumentReadOnly(false);
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly('text-box');
});

// Remove .skip when the front-end is ready. 
// Issue link: https://github.com/umbraco/Umbraco-CMS/issues/19395
test.skip('can edit property values with UI write permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedText = 'Updated test text';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermissionAndWritePropertyValuePermission(userGroupName, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.enterTextstring(updatedText);
  await umbracoUi.content.clickSaveButton();

  // Assert
  const documentData = await umbracoApi.document.getByName(documentName);
  expect(documentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(documentData.values[0].value).toEqual(updatedText);
});

test('cannot see property values with only UI write but no UI read permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermissionAndWritePropertyValuePermission(userGroupName, true, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('text-box', false);
});