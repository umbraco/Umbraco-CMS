import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('User Group tests', () => {
  const userGroupName = 'TestUserGroupName';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.userGroup.goToSection(ConstantHelper.sections.users);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  });

  test('can create an empty user group', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickCreateButton();
    await umbracoUi.userGroup.enterUserGroupName(userGroupName);
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    expect(await umbracoApi.userGroup.doesNameExist(userGroupName)).toBeTruthy();
    // Checks if the user group was created in the UI as well
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.isUserGroupWithNameVisible(userGroupName);
  })

  test('can rename a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const oldUserGroupName = 'OldUserGroupName';
    await umbracoApi.userGroup.ensureNameNotExists(oldUserGroupName);
    await umbracoApi.userGroup.createEmptyUserGroup(oldUserGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(oldUserGroupName);
    await umbracoUi.userGroup.enterUserGroupName(userGroupName);
    await umbracoUi.userGroup.clickSaveButton();
    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    expect(await umbracoApi.userGroup.doesNameExist(userGroupName)).toBeTruthy();
    // Checks if the user group was created in the UI as well
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.isUserGroupWithNameVisible(userGroupName);
    await umbracoUi.userGroup.isUserGroupWithNameVisible(oldUserGroupName, false);
  });

  test('can update a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createSimpleUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.clickBrowseNodePermission();
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    // We reload the page to make sure that the permission is saved in the frontend
    await umbracoUi.reloadPage();
    await umbracoUi.userGroup.doesUserGroupHavePermission('Browse Node');
    const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
    expect(userGroupData.fallbackPermissions).toContain('Umb.Document.Read');
  });

  test('can delete a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createSimpleUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.clickDeleteThreeDotsButton();
    await umbracoUi.userGroup.clickDeleteExactButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    expect(await umbracoApi.userGroup.doesNameExist(userGroupName)).toBeFalsy();
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.isUserGroupWithNameVisible(userGroupName, false);
  });

  // TODO: it is not possible to add a section to a user group in the frontend
  test.skip('can add a section to a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.clickAddSectionsButton();
    await umbracoUi.userGroup.addSectionWithNameToUserGroup('Content');
    await umbracoUi.userGroup.clickSubmitButton();
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Content');
  })

  // TODO: it is not possible to add a section to a user group in the frontend
  test.skip('can add multiple sections to a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createSimpleUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.clickAddSectionsButton();
    await umbracoUi.userGroup.addSectionWithNameToUserGroup('Media');
    await umbracoUi.userGroup.clickSubmitButton();
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Content');
    await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Media');
  });

  test('can remove a section from a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createSimpleUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.removeSectionFromUserGroup('Content');
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Content', false);
    const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
    expect(userGroupData.sections).toEqual([])
  });

  // TODO: Create test after the content builder is available
  test.skip('can add a content start node to a user group', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });

  // TODO: Create test after the content builder is available
  test.skip('can add multiple content start nodes to a user group', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });

  // TODO: Create test after the content builder is available
  test.skip('can remove a content start node from a user group ', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });

  // TODO: Create test after the Media builder is available, The button choose is not clickable
  test.skip('can add a media start node to a user group', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });

  // TODO: Create test after the Media builder is available
  test.skip('can add multiple media start nodes to a user group', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });

  // TODO: Create test after the Media builder is available
  test.skip('can remove a media start node from a user group ', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });

  test('can enable all permissions for a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
    const allPermissions = [
      'Umb.Document.Read',
      'Umb.Document.CreateBlueprint',
      'Umb.Document.Delete',
      'Umb.Document.Create',
      'Umb.Document.Notifications',
      'Umb.Document.Publish',
      'Umb.Document.Permissions',
      'Umb.Document.Unpublish',
      'Umb.Document.Update',
      'Umb.Document.Duplicate',
      'Umb.Document.Move',
      'Umb.Document.Sort',
      'Umb.Document.CultureAndHostnames',
      'Umb.Document.PublicAccess',
      'Umb.Document.Rollback'
    ];

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

    await umbracoUi.userGroup.clickAllPermissionsOnAUserGroup();
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.reloadPage();
    await umbracoUi.userGroup.doesUserGroupHaveAllPermissionsEnabled();
    const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
    expect(userGroupData.fallbackPermissions).toEqual(allPermissions);
  });

  // TODO: Create test after the content builder is available
  test.skip('can add permission to a specific document for a user group', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });
});
