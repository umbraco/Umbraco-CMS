import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('User Group Tests', () => {
  const userGroupName = "UserGroupTest";

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.userGroup.ensureUserGroupNameNotExists(userGroupName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.userGroup.ensureUserGroupNameNotExists(userGroupName);
  });

  test('can create a user group', async ({page, umbracoApi, umbracoUi}) => {
    const sections = ["Umb.Section.Content",
      "Umb.Section.Forms",
      "Umb.Section.Media"];

    await umbracoApi.userGroup.createUserGroup(userGroupName, true, sections);

    // Assert
    await expect(umbracoApi.userGroup.doesUserGroupWithNameExists(userGroupName)).toBeTruthy();
  });

  test('can update a user group', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.userGroup.createUserGroup('UserGroupNameTest', true);

    const userGroupData = await umbracoApi.userGroup.getUserGroupByName('UserGroupNameTest');

    // Updates name of the user group
    userGroupData.name = userGroupName;
    await umbracoApi.userGroup.updateUserGroupById(userGroupData.id, userGroupData);


    // Assert
    const updatedUserGroupData = await umbracoApi.userGroup.getUserGroupById(userGroupData.id);
    await expect(updatedUserGroupData.name == userGroupName).toBeTruthy();
  });

  test('can delete a user group', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.userGroup.createUserGroup(userGroupName, true);

    const userGroupData = await umbracoApi.userGroup.getUserGroupByName(userGroupName);

    await umbracoApi.userGroup.deleteUserGroupById(userGroupData.id);

    // Assert
    await expect(await umbracoApi.userGroup.doesUserGroupWithNameExists(userGroupName)).toBeFalsy();
  });
});
