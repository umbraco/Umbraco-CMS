import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('User Group Tests', () => {
  let userGroupId = "";
  const userGroupName = "UserGroupTest";

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.userGroup.delete(userGroupId);
  });

  test('can create a user group', async ({umbracoApi}) => {
    // Arrange
    const sections = ["Umb.Section.Content",
      "Umb.Section.Forms",
      "Umb.Section.Media"];

      // Act
    userGroupId = await umbracoApi.userGroup.create(userGroupName, true, sections);

    // Assert
    expect(umbracoApi.userGroup.exist(userGroupId)).toBeTruthy();
  });

  test('can update a user group', async ({umbracoApi}) => {
    // Arrange
    userGroupId = await umbracoApi.userGroup.create('UserGroupNameTest', true);
    const userGroupData = await umbracoApi.userGroup.get(userGroupId);
    userGroupData.name = userGroupName;

    // Act
    await umbracoApi.userGroup.update(userGroupId, userGroupData);

    // Assert
    const updatedUserGroupData = await umbracoApi.userGroup.get(userGroupId);
    expect(updatedUserGroupData.name).toEqual(userGroupName);
  });

  test('can delete a user group', async ({umbracoApi}) => {
    // Arrange
    userGroupId = await umbracoApi.userGroup.create(userGroupName, true);

    // Act
    await umbracoApi.userGroup.delete(userGroupId);

    // Assert
    expect(await umbracoApi.userGroup.exist(userGroupId)).toBeFalsy();
  });
});
