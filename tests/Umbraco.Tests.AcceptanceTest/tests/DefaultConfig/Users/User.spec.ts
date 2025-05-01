import {NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const nameOfTheUser = 'TestUser';
const userEmail = 'TestUser@EmailTest.test';
const defaultUserGroupName = 'Writers';
let userCount = null;

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoApi.user.ensureNameNotExists(nameOfTheUser);
});

test.afterEach(async ({umbracoApi, umbracoUi}) => {
  // Waits so we can try to avoid db locks
  await umbracoUi.waitForTimeout(500);
  await umbracoApi.user.ensureNameNotExists(nameOfTheUser);
});

test('can create a user', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickCreateButton();
  await umbracoUi.user.clickUserButton();
  await umbracoUi.user.enterNameOfTheUser(nameOfTheUser);
  await umbracoUi.user.enterUserEmail(userEmail);
  await umbracoUi.user.clickChooseButton();
  await umbracoUi.user.clickButtonWithName(defaultUserGroupName);
  await umbracoUi.user.clickChooseModalButton();
  await umbracoUi.user.clickCreateUserButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeTruthy();
});

test('can rename a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'WrongName';
  await umbracoApi.user.ensureNameNotExists(wrongName);
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(wrongName, wrongName + userEmail, [userGroup.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(wrongName);
  await umbracoUi.user.enterUpdatedNameOfUser(nameOfTheUser);
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeTruthy();
});

test('can delete a user', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickActionButton();
  await umbracoUi.user.clickDeleteButton();
  await umbracoUi.user.clickConfirmToDeleteButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeFalsy();
  // Checks if the user is deleted from the list
  await umbracoUi.user.clickUsersMenu();
  await umbracoUi.user.isUserVisible(nameOfTheUser, false);
});

test('can add multiple user groups to a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondUserGroupName = 'Translators';
  const userGroupWriters = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userGroupTranslators = await umbracoApi.userGroup.getByName(secondUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroupWriters.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickChooseUserGroupsButton();
  await umbracoUi.user.clickButtonWithName(secondUserGroupName);
  await umbracoUi.user.clickChooseModalButton();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(nameOfTheUser, [userGroupWriters.id, userGroupTranslators.id])).toBeTruthy();
});

test('can remove a user group from a user', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickRemoveButtonForUserGroupWithName(defaultUserGroupName);
  await umbracoUi.user.clickConfirmRemoveButton();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  expect(userData.userGroupIds).toEqual([]);
});

test('can update culture for a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const danishIsoCode = 'da-dk';
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.selectUserLanguage('Dansk (Danmark)');
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  expect(userData.languageIsoCode).toEqual(danishIsoCode);
});

test('can add a content start node to a user', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickChooseContentStartNodeButton();
  await umbracoUi.user.clickLabelWithName(documentName);
  await umbracoUi.user.clickChooseContainerButton();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesUserContainContentStartNodeIds(nameOfTheUser, [documentId])).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can add multiple content start nodes for a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  const secondDocumentName = 'SecondDocument';
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  // Adds the content start node to the user
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  userData.documentStartNodeIds.push({id: documentId});
  await umbracoApi.user.update(userId, userData);
  const secondDocumentId = await umbracoApi.document.createDefaultDocument(secondDocumentName, documentTypeId);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickChooseContentStartNodeButton();
  await umbracoUi.user.clickLabelWithName(secondDocumentName);
  await umbracoUi.user.clickChooseContainerButton();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesUserContainContentStartNodeIds(nameOfTheUser, [documentId, secondDocumentId])).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can remove a content start node from a user', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  // Adds the content start node to the user
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  userData.documentStartNodeIds.push({id: documentId});
  await umbracoApi.user.update(userId, userData);
  expect(await umbracoApi.user.doesUserContainContentStartNodeIds(nameOfTheUser, [documentId])).toBeTruthy();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickRemoveButtonForContentNodeWithName(documentName);
  await umbracoUi.user.clickConfirmRemoveButton();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesUserContainContentStartNodeIds(nameOfTheUser, [documentId])).toBeFalsy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can add media start nodes for a user', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaName = 'TestMediaFile';
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaName);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickChooseMediaStartNodeButton();
  await umbracoUi.user.selectMediaWithName(mediaName);
  await umbracoUi.user.clickChooseModalButton();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesUserContainMediaStartNodeIds(nameOfTheUser, [mediaId])).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can add multiple media start nodes for a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  const mediaName = 'TestMediaFile';
  const secondMediaName = 'SecondMediaFile';
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.media.ensureNameNotExists(secondMediaName);
  const firstMediaId = await umbracoApi.media.createDefaultMediaFile(mediaName);
  const secondMediaId = await umbracoApi.media.createDefaultMediaFile(secondMediaName);
  // Adds the media start node to the user
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  userData.mediaStartNodeIds.push({id: firstMediaId});
  await umbracoApi.user.update(userId, userData);
  expect(await umbracoApi.user.doesUserContainMediaStartNodeIds(nameOfTheUser, [firstMediaId])).toBeTruthy();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickChooseMediaStartNodeButton();
  await umbracoUi.user.selectMediaWithName(secondMediaName);
  await umbracoUi.user.clickChooseModalButton();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesUserContainMediaStartNodeIds(nameOfTheUser, [firstMediaId, secondMediaId])).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.media.ensureNameNotExists(secondMediaName);
});

test('can remove a media start node from a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  const mediaName = 'TestMediaFile';
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaName);
  // Adds the media start node to the user
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  userData.mediaStartNodeIds.push({id: mediaId});
  await umbracoApi.user.update(userId, userData);
  expect(await umbracoApi.user.doesUserContainMediaStartNodeIds(nameOfTheUser, [mediaId])).toBeTruthy();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickRemoveButtonForMediaNodeWithName(mediaName);
  await umbracoUi.user.clickConfirmRemoveButton();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  expect(await umbracoApi.user.doesUserContainMediaStartNodeIds(nameOfTheUser, [mediaId])).toBeFalsy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can allow access to all documents for a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickAllowAccessToAllDocumentsToggle();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  expect(userData.hasDocumentRootAccess).toBeTruthy()
});

test('can allow access to all media for a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickAllowAccessToAllMediaToggle();
  await umbracoUi.user.clickSaveButton();

  // Assert
  //await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.user.isErrorNotificationVisible(false);
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  expect(userData.hasMediaRootAccess).toBeTruthy();
});

test('can see if the user has the correct access based on content start nodes', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  // Adds the content start node to the user
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  userData.documentStartNodeIds.push({id: documentId});
  await umbracoApi.user.update(userId, userData);
  expect(await umbracoApi.user.doesUserContainContentStartNodeIds(nameOfTheUser, [documentId])).toBeTruthy();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  // Currently this wait is necessary
  await umbracoUi.waitForTimeout(2000);

  // Assert
  await umbracoUi.user.doesUserHaveAccessToContentNode(documentName);

  // Clean
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see if the user has the correct access based on media start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  const mediaName = 'TestMediaFile';
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaName);
  // Adds the media start node to the user
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  userData.mediaStartNodeIds.push({id: mediaId});
  await umbracoApi.user.update(userId, userData);
  expect(await umbracoApi.user.doesUserContainMediaStartNodeIds(nameOfTheUser, [mediaId])).toBeTruthy();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);

  // Assert
  await umbracoUi.user.doesUserHaveAccessToMediaNode(mediaName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can change password for a user', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userPassword = 'TestPassword';
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickActionButton();
  await umbracoUi.user.clickChangePasswordButton();
  await umbracoUi.user.updatePassword(userPassword);

  // Assert
  await umbracoUi.user.isPasswordUpdatedForUserWithId(userId);
});

test('can disable a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const disabledStatus = 'Disabled';
  // We need to create a new user because the "TestUser" is used in other tests, which can affect if the user is disabled or not
  const newTestUser = 'TestUserNumberTwo';
  await umbracoApi.user.ensureNameNotExists(newTestUser);
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(newTestUser, newTestUser + userEmail, [userGroup.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(newTestUser);
  await umbracoUi.user.clickActionButton();
  await umbracoUi.user.clickDisableButton();
  await umbracoUi.user.clickConfirmDisableButton();

  // Assert
  await umbracoUi.user.doesSuccessNotificationHaveText(newTestUser + NotificationConstantHelper.success.userDisabled);
  expect(umbracoUi.user.isUserDisabledTextVisible()).toBeTruthy();
  const userData = await umbracoApi.user.getByName(newTestUser);
  expect(userData.state).toBe(disabledStatus);

  // Clean
  await umbracoApi.user.ensureNameNotExists(newTestUser);
});

test('can enable a user', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inactiveStatus = 'Inactive';
  const newTestUser = 'TestUserNumberTwo';
  await umbracoApi.user.ensureNameNotExists(newTestUser);
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(newTestUser, newTestUser + userEmail, [userGroup.id]);
  await umbracoApi.user.disable([userId]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(newTestUser);
  await umbracoUi.user.clickActionButton();
  await umbracoUi.user.clickEnableButton();
  await umbracoUi.user.clickConfirmEnableButton();

  // Assert
  // TODO: Unskip when it shows userEnabled/userInactive instead of userDisabled
  // await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.userEnabled);
  await umbracoUi.user.isUserActiveTextVisible();
  // The state of the user is not enabled. The reason for this is that the user has not logged in, resulting in the state Inactive.
  const userData = await umbracoApi.user.getByName(newTestUser);
  expect(userData.state).toBe(inactiveStatus);

  // Clean
  await umbracoApi.user.ensureNameNotExists(newTestUser);
});

test('can add an avatar to a user', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  const filePath = './fixtures/mediaLibrary/Umbraco.png';
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.changePhotoWithFileChooser(filePath);

  // Assert
  await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.avatarUploaded);
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  expect(userData.avatarUrls).not.toHaveLength(0);
});

test('can remove an avatar from a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  await umbracoApi.user.addDefaultAvatarImageToUser(userId);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickRemovePhotoButton();

  // Assert
  await umbracoUi.user.doesSuccessNotificationHaveText(NotificationConstantHelper.success.avatarDeleted);
  const userData = await umbracoApi.user.getByName(nameOfTheUser);
  expect(userData.avatarUrls).toHaveLength(0);
});

test('can see if the inactive label is removed from the admin user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userLabel = 'Active';
  const currentUser = await umbracoApi.user.getCurrentUser();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(currentUser.name);

  // Assert
  await umbracoUi.user.isTextWithExactNameVisible(userLabel);
  const userData = await umbracoApi.user.getByName(currentUser.name);
  expect(userData.state).toBe(userLabel);
});

test('can search for a user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  userCount = await umbracoApi.user.getUsersCount();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.doesUserSectionContainUserAmount(userCount);
  await umbracoUi.user.searchInUserSection(nameOfTheUser);

  // Assert
  // Wait for filtering to be done
  await umbracoUi.waitForTimeout(200);
  const userData = await umbracoApi.user.filterByText(nameOfTheUser);
  await umbracoUi.user.doesUserSectionContainUserAmount(userData.total);
  await umbracoUi.user.doesUserSectionContainUserWithText(nameOfTheUser);
});

test('can filter by status', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inactiveStatus = 'Inactive';
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  userCount = await umbracoApi.user.getUsersCount();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.doesUserSectionContainUserAmount(userCount);
  await umbracoUi.user.filterByStatusName(inactiveStatus);

  // Assert
  // Wait for filtering to be done
  await umbracoUi.waitForTimeout(200);
  const userData = await umbracoApi.user.filterByUserStates(inactiveStatus);
  await umbracoUi.user.doesUserSectionContainUserAmount(userData.total);
  await umbracoUi.user.doesUserSectionContainUserWithText(nameOfTheUser);
  await umbracoUi.user.doesUserSectionContainUserWithText(inactiveStatus);
});

test('can filter by user groups', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  userCount = await umbracoApi.user.getUsersCount();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.doesUserSectionContainUserAmount(userCount);
  await umbracoUi.user.filterByGroupName(defaultUserGroupName);

  // Assert
  // Wait for filtering to be done
  await umbracoUi.waitForTimeout(200);
  const userData = await umbracoApi.user.filterByUserGroupIds(userGroup.id);
  await umbracoUi.user.doesUserSectionContainUserAmount(userData.total);
  await umbracoUi.user.doesUserSectionContainUserWithText(defaultUserGroupName);
});

test('can order by newest user', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  userCount = await umbracoApi.user.getUsersCount();
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.doesUserSectionContainUserAmount(userCount);
  await umbracoUi.user.orderByNewestUser();

  // Assert
  // Wait for filtering to be done
  await umbracoUi.waitForTimeout(200);

  await umbracoUi.user.doesUserSectionContainUserAmount(userCount);
  await umbracoUi.user.isUserWithNameTheFirstUserInList(nameOfTheUser);
});

// TODO: Sometimes the frontend does not switch from grid to table, or table to grid.
test.skip('can change from grid to table view', async ({page, umbracoApi, umbracoUi}) => {
});
