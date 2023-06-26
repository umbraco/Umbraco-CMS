import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('User Tests', () => {
  const userEmail = "user@email.com";
  const userName = "UserTests";

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.user.ensureUserNameNotExists(userName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.user.ensureUserNameNotExists(userName);
  });

  test('can create a user', async ({page, umbracoApi, umbracoUi}) => {
    // Gets the id for the writers userGroup
    const userGroup = await umbracoApi.userGroup.getUserGroupByName("Writers");
    const userGroupId = [userGroup.id];

    await umbracoApi.user.createUser(userEmail, userName, userGroupId);

    // Assert
    await expect(await umbracoApi.user.doesUserWithNameExist(userName)).toBeTruthy();
  });

  test('can update a user', async ({page, umbracoApi, umbracoUi}) => {
    // Gets userGroup data for Writers and Translators
    const userGroup = await umbracoApi.userGroup.getUserGroupByName("Writers");
    const anotherUserGroup = await umbracoApi.userGroup.getUserGroupByName("Translators");

    const userGroupId = [userGroup.id];

    await umbracoApi.user.createUser(userEmail, userName, userGroupId);

    const userData = await umbracoApi.user.getUserByName(userName);

    const newUserGroupData = [
      userGroup.id,
      anotherUserGroup.id
    ];

    userData.userGroupIds = newUserGroupData;

    await umbracoApi.user.updateUserById(userData.id, userData);

    // Assert
    await umbracoApi.user.doesUserWithNameExist(userName);
    // Checks if the user was updated with another userGroupID
    const updatedUser = await umbracoApi.user.getUserByName(userName);
    await expect(updatedUser.userGroupIds.toString() == newUserGroupData.toString()).toBeTruthy();
  });

  test('can delete a user', async ({page, umbracoApi, umbracoUi}) => {
    const userGroupData = await umbracoApi.userGroup.getUserGroupByName("Writers");

    const userData = [
      userGroupData.id
    ];

    await umbracoApi.user.createUser(userEmail, userName, userData);

    await expect(await umbracoApi.user.doesUserWithNameExist(userName)).toBeTruthy();

    await umbracoApi.user.deleteUserByName(userName);

    // Assert
    await expect(await umbracoApi.user.doesUserWithNameExist(userName)).toBeFalsy();
  });
});
