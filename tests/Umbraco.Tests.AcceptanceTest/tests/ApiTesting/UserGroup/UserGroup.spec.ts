import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('User Group Tests', () => {
  let userGroupId = "";
  const userGroupName = "UserGroupTest";

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.userGroup.delete(userGroupId);
  });

  test('can create a user group', async ({page, umbracoApi, umbracoUi}) => {
    const sections = ["Umb.Section.Content",
      "Umb.Section.Forms",
      "Umb.Section.Media"];

    userGroupId = await umbracoApi.userGroup.create(userGroupName, true, sections);

    // Assert
    await expect(umbracoApi.userGroup.exist(userGroupId)).toBeTruthy();
  });

  test('can update a user group', async ({page, umbracoApi, umbracoUi}) => {
    userGroupId = await umbracoApi.userGroup.create('UserGroupNameTest', true);

    const userGroupData = await umbracoApi.userGroup.get(userGroupId);

    // Updates name of the user group
    userGroupData.name = userGroupName;
    await umbracoApi.userGroup.update(userGroupId, userGroupData);

    // Assert
    const updatedUserGroupData = await umbracoApi.userGroup.get(userGroupId);
    await expect(updatedUserGroupData.name).toEqual(userGroupName);
  });

  test('can delete a user group', async ({page, umbracoApi, umbracoUi}) => {
    userGroupId = await umbracoApi.userGroup.create(userGroupName, true);

    await umbracoApi.userGroup.delete(userGroupId);

    // Assert
    await expect(await umbracoApi.userGroup.exist(userGroupId)).toBeFalsy();
  });
});
