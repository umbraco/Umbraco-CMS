import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import * as crypto from 'crypto';

test.describe('User Avatar Tests', () => {
  // User
  let userId = '';
  let userGroupId = '';
  const userEmail = 'userAvatar@acceptance.test';
  const userName = 'UserAvatarTests';
  // Avatar
  const avatarFileId = crypto.randomUUID();
  const avatarName = 'Umbraco.png';
  const mimeType = 'image/png';
  const avatarFilePath = './fixtures/mediaLibrary/Umbraco.png';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.user.ensureNameNotExists(userName);
    await umbracoApi.temporaryFile.delete(avatarFileId);
    const userGroupData = await umbracoApi.userGroup.getByName('Writers');
    userGroupId = userGroupData.id;
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.user.ensureNameNotExists(userName);
    await umbracoApi.temporaryFile.delete(avatarFileId);
  });

  test('can add an avatar to a user', async ({umbracoApi}) => {
    // Arrange
    userId = await umbracoApi.user.createDefaultUser(userName, userEmail, userGroupId);
    await umbracoApi.temporaryFile.create(avatarFileId, avatarName, mimeType, avatarFilePath);

    // Act
    await umbracoApi.user.addAvatar(userId, avatarFileId);

    // Assert
    // Checks if the avatar was added to the user
    const userDataWithAvatar = await umbracoApi.user.get(userId);
    expect(userDataWithAvatar.avatarUrls.length !== 0).toBeTruthy();
  });

  test('can remove an avatar from a user', async ({umbracoApi}) => {
    // Arrange
    userId = await umbracoApi.user.createDefaultUser(userName, userEmail, userGroupId);
    await umbracoApi.temporaryFile.create(avatarFileId, avatarName, mimeType, avatarFilePath);
    await umbracoApi.user.addAvatar(userId, avatarFileId);

    // Checks if the avatar was added to the user
    const userDataWithAvatar = await umbracoApi.user.get(userId);
    expect(userDataWithAvatar.avatarUrls.length !== 0).toBeTruthy();

    // Act
    await umbracoApi.user.removeAvatar(userId);

    // Assert
    // Checks if the avatar was removed from the user
    const userDataWithoutAvatar = await umbracoApi.user.get(userId);
    expect(userDataWithoutAvatar.avatarUrls.length).toEqual(0);
  });
});
