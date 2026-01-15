import { expect } from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

const userGroupName = 'TestPropertyValuePermission';
let userGroupId = null;

const documentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Block List';
const elementTypeName = 'BlockListElement';
const propertyInBlock = 'Textstring';
const groupName = 'testGroup';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(documentName);
  const textStringData = await umbracoApi.dataType.getByName(propertyInBlock);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, propertyInBlock, textStringData.id);
});

test.afterEach(async ({umbracoApi}) => {
    // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can see property values in block list with UI read but not UI write permission', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockListEditor(documentName, elementTypeId, documentTypeName, customDataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadPermissionAndReadPropertyValuePermission(userGroupName, true, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('block-list', true);
  await umbracoUi.content.clickEditBlockListBlockButton();

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly('text-box');
});

// Remove .skip when the front-end is ready. 
// Issue link: https://github.com/umbraco/Umbraco-CMS/issues/19395
test.skip('can edit property values in block list with UI write permission', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedText = 'Updated test text';
  await umbracoApi.document.createDefaultDocumentWithABlockListEditor(documentName, elementTypeId, documentTypeName, customDataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermissionAndWritePropertyValuePermission(userGroupName, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('block-list', true);
  await umbracoUi.content.clickEditBlockListBlockButton();
  await umbracoUi.content.enterTextstring(updatedText);
  await umbracoUi.content.clickUpdateButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const documentData = await umbracoApi.document.getByName(documentName);
  expect(documentData.values[0].value.contentData[0].values[0].value).toEqual(updatedText);
});

test('cannot see property values in block list with only UI write but no UI read permission', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockListEditor(documentName, elementTypeId, documentTypeName, customDataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermissionAndWritePropertyValuePermission(userGroupName, true, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('block-list', false);
});

test('can see property values in block grid with UI read but not UI write permission', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditor(documentName, elementTypeId, documentTypeName, customDataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithReadPermissionAndReadPropertyValuePermission(userGroupName, true, true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('block-grid', true);
  await umbracoUi.content.clickEditBlockGridBlockButton();

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly('text-box');
});

// Remove .skip when the front-end is ready. 
// Issue link: https://github.com/umbraco/Umbraco-CMS/issues/19395
test.skip('can edit property values in block grid with UI write permission', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedText = 'Updated test text';
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditor(documentName, elementTypeId, documentTypeName, customDataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermissionAndWritePropertyValuePermission(userGroupName, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('block-grid', true);
  await umbracoUi.content.clickEditBlockGridBlockButton();
  await umbracoUi.content.enterTextstring(updatedText);
  await umbracoUi.content.clickUpdateButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const documentData = await umbracoApi.document.getByName(documentName);
  expect(documentData.values[0].value.contentData[0].values[0].value).toEqual(updatedText);
});

test('cannot see property values in block grid with only UI write but no UI read permission', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditor(documentName, elementTypeId, documentTypeName, customDataTypeName);
  userGroupId = await umbracoApi.userGroup.createUserGroupWithUpdatePermissionAndWritePropertyValuePermission(userGroupName, true, true, false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isPropertyEditorUiWithNameVisible('block-grid', false);
});