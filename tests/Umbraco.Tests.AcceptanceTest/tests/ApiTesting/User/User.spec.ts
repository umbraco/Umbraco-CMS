import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('User Tests', () => {
  let userId = '';
  let userGroupId = '';
  const userEmail = 'user@acceptance.test';
  const userName = 'UserTests';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.user.ensureNameNotExists(userName);
    const userGroupData = await umbracoApi.userGroup.getByName('Writers');
    userGroupId = userGroupData.id;
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.user.ensureNameNotExists(userName);
  });

  test('can create a user', async ({umbracoApi}) => {
    // Act
    userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupId]);

    // Assert
    expect(await umbracoApi.user.doesExist(userId)).toBeTruthy();
  });

  test('can update a user', async ({umbracoApi}) => {
    // Arrange
    const anotherUserGroup = await umbracoApi.userGroup.getByName("Translators");
    userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupId]);
    const userData = await umbracoApi.user.get(userId);
    const newUserGroupData = [
      userGroupId,
      anotherUserGroup.id
    ];
    userData.userGroupIds = newUserGroupData;

    // Act
    await umbracoApi.user.update(userId, userData);

    // Assert
    await umbracoApi.user.doesExist(userId);
    // Checks if the user was updated with another userGroupID
    const updatedUser = await umbracoApi.user.get(userId);
    expect(updatedUser.userGroupIds.toString()).toEqual(newUserGroupData.toString());
  });

  test('can delete a user', async ({umbracoApi}) => {
    // Arrange
    userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupId]);
    expect(await umbracoApi.user.doesExist(userId)).toBeTruthy();

    // Act
    await umbracoApi.user.delete(userId);

    // Assert
    expect(await umbracoApi.user.doesExist(userId)).toBeFalsy();
  });
});
