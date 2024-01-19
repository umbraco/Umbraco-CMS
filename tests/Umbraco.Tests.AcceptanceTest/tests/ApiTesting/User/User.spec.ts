import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('User Tests', () => {
  let userId = "";
  const userEmail = "user@email.com";
  const userName = "UserTests";

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.user.ensureNameNotExists(userName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.user.ensureNameNotExists(userName);
  });

  test('can create a user', async ({umbracoApi}) => {
    // Arrange
    // Gets the id for the writers userGroup
    const userGroup = await umbracoApi.userGroup.getByName("Writers");
    const userGroupId = [userGroup.id];

    // Act
    userId = await umbracoApi.user.create(userEmail, userName, userGroupId);

    // Assert
    expect(await umbracoApi.user.exists(userId)).toBeTruthy();
  });

  test('can update a user', async ({umbracoApi}) => {
    // Arrange
    // Gets userGroup data for Writers and Translators
    const userGroup = await umbracoApi.userGroup.getByName("Writers");
    const anotherUserGroup = await umbracoApi.userGroup.getByName("Translators");
    const userGroupId = [userGroup.id];
    userId = await umbracoApi.user.create(userEmail, userName, userGroupId);
    const userData = await umbracoApi.user.get(userId);
    const newUserGroupData = [
      userGroup.id,
      anotherUserGroup.id
    ];
    userData.userGroupIds = newUserGroupData;

    // Act
    await umbracoApi.user.update(userId, userData);

    // Assert
    await umbracoApi.user.exists(userId);
    // Checks if the user was updated with another userGroupID
    const updatedUser = await umbracoApi.user.get(userId);
    expect(updatedUser.userGroupIds.toString()).toEqual(newUserGroupData.toString());
  });

  test('can delete a user', async ({umbracoApi}) => {
    // Arrange
    const userGroupData = await umbracoApi.userGroup.getByName("Writers");
    const userData = [
      userGroupData.id
    ];

    userId = await umbracoApi.user.create(userEmail, userName, userData);
    expect(await umbracoApi.user.exists(userId)).toBeTruthy();

    // Act
    await umbracoApi.user.delete(userId);

    // Assert
    expect(await umbracoApi.user.exists(userId)).toBeFalsy();
  });
});
