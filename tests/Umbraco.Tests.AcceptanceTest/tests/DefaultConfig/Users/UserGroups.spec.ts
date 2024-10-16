import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const allPermissions = {
  uiPermission:
    ['Browse Node',
      'Create Document Blueprint',
      'Delete',
      'Create',
      'Notifications',
      'Publish',
      'Set permissions',
      'Unpublish',
      'Update',
      'Duplicate',
      'Move to',
      'Sort children',
      'Culture and Hostnames',
      'Public Access',
      'Rollback'],
  verbPermission: [
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
  ]
};

const englishLanguage = 'English (United States)';

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

  test('can create an empty user group', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickCreateUserGroupButton();
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
    await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.clickPermissionsByName([allPermissions.uiPermission[0]]);
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    // We reload the page to make sure that the permission is saved in the frontend
    await umbracoUi.reloadPage();
    await umbracoUi.userGroup.doesUserGroupHavePermission(allPermissions.uiPermission[0]);
    const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
    expect(userGroupData.fallbackPermissions).toContain(allPermissions.verbPermission[0]);
  });

  test('can delete a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.clickDeleteButton();
    await umbracoUi.userGroup.clickConfirmToDeleteButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    expect(await umbracoApi.userGroup.doesNameExist(userGroupName)).toBeFalsy();
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.isUserGroupWithNameVisible(userGroupName, false);
  });

  test('can add a section to a user group', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.addSectionWithNameToUserGroup('Content');
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Content');
  })

  test('can add multiple sections to a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.addSectionWithNameToUserGroup('Media');
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Content');
    await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Media');
  });

  test('can remove a section from a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.removeSectionFromUserGroup('Content');
    await umbracoUi.userGroup.clickConfirmRemoveButton();
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Content', false);
    const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
    expect(userGroupData.sections).toEqual([])
  });

  test('can add a language to a user group', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.addLanguageToUserGroup(englishLanguage);
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.userGroup.doesUserGroupContainLanguage(englishLanguage);
    expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'en-US')).toBeTruthy();
  })

  test('can enable all languages for a user group', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.clickAllowAccessToAllLanguages();
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    expect(await umbracoApi.userGroup.doesUserGroupContainAccessToAllLanguages(userGroupName)).toBeTruthy();
  })

  test('can add multiple languages to a user group', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createUserGroupWithLanguage(userGroupName, 'en-US');
    const danishLanguage = 'Danish';
    await umbracoApi.language.ensureNameNotExists(danishLanguage);
    await umbracoApi.language.createDanishLanguage();

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.addLanguageToUserGroup(danishLanguage);
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.userGroup.doesUserGroupContainLanguage(englishLanguage);
    await umbracoUi.userGroup.doesUserGroupContainLanguage(danishLanguage);
    expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'en-US')).toBeTruthy();
    expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'da')).toBeTruthy();

    // Clean
    await umbracoApi.language.ensureNameNotExists(danishLanguage);
  })

  test('can remove language from a user group', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createUserGroupWithLanguage(userGroupName, 'en-US');
    expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'en-US')).toBeTruthy();

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.clickRemoveLanguageFromUserGroup(englishLanguage);
    await umbracoUi.userGroup.clickConfirmRemoveButton();
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.userGroup.doesUserGroupContainLanguage(englishLanguage, false)
    expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'en-US')).toBeFalsy();
  })

  test('can add a content start node to a user group', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
    const documentTypeName = 'TestDocumentType';
    const documentName = 'TestDocument';
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

    // Act
    await umbracoUi.user.clickChooseContentStartNodeButton();
    await umbracoUi.user.clickLabelWithName(documentName);
    await umbracoUi.user.clickChooseContainerButton();
    await umbracoUi.user.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoApi.userGroup.doesUserGroupContainContentStartNodeIds(userGroupName, [documentId]);

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(documentTypeId);
    await umbracoApi.document.ensureNameNotExists(documentId);
  });

  // TODO: Create test after the content builder is available
  test('can enable access to all content from a user group ', async ({page, umbracoApi, umbracoUi}) => {
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
  test.skip('can remove a media start node from a user group ', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });

  // TODO: Create test after the Media builder is available
  test.skip('can enable access to all media in a user group ', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });

  test('can enable all permissions for a user group', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);

    // Act
    await umbracoUi.userGroup.clickUserGroupsTabButton();
    await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
    await umbracoUi.userGroup.clickPermissionsByName(allPermissions.uiPermission);
    await umbracoUi.userGroup.clickSaveButton();

    // Assert
    await umbracoUi.userGroup.isSuccessNotificationVisible();
    await umbracoUi.reloadPage();
    await umbracoUi.userGroup.doesUserGroupHavePermissionEnabled(allPermissions.uiPermission);
    const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
    expect(userGroupData.fallbackPermissions).toEqual(allPermissions.verbPermission);
  });

  // TODO: Create test after the content builder is available
  test.skip('can add permission to a specific document for a user group', async ({page, umbracoApi, umbracoUi}) => {
    await page.pause()
  });
});
