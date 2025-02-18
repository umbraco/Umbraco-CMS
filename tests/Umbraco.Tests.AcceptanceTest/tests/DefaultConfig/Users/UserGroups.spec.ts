import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
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
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickCreateLink();
  await umbracoUi.userGroup.enterUserGroupName(userGroupName);
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.userGroup.doesNameExist(userGroupName)).toBeTruthy();
  // Checks if the user group was created in the UI as well
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.isUserGroupWithNameVisible(userGroupName);
});

test('can rename a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const oldUserGroupName = 'OldUserGroupName';
  await umbracoApi.userGroup.ensureNameNotExists(oldUserGroupName);
  await umbracoApi.userGroup.createEmptyUserGroup(oldUserGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(oldUserGroupName);

  // Act
  await umbracoUi.userGroup.enterUserGroupName(userGroupName);
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesNameExist(userGroupName)).toBeTruthy();
  // Checks if the user group was created in the UI as well
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.isUserGroupWithNameVisible(userGroupName);
  await umbracoUi.userGroup.isUserGroupWithNameVisible(oldUserGroupName, false);
});

test('can update a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickPermissionsByName([allPermissions.uiPermission[0]]);
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.reloadPage();
  await umbracoUi.userGroup.doesUserGroupHavePermission(allPermissions.uiPermission[0]);
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.fallbackPermissions).toContain(allPermissions.verbPermission[0]);
});

test('can delete a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickActionsButton();
  await umbracoUi.userGroup.clickDeleteButton();
  await umbracoUi.userGroup.clickConfirmToDeleteButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  expect(await umbracoApi.userGroup.doesNameExist(userGroupName)).toBeFalsy();
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.isUserGroupWithNameVisible(userGroupName, false);
});

test('can add a section to a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.addSectionWithNameToUserGroup('Content');
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Content');
})

test('can add multiple sections to a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.addSectionWithNameToUserGroup('Media');
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Content');
  await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Media');
});

test('can remove a section from a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickRemoveSectionFromUserGroup('Content');
  await umbracoUi.userGroup.clickConfirmRemoveButton();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.doesUserGroupHaveSection(userGroupName, 'Content', false);
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.sections).toEqual([]);
});

test('can add a language to a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.addLanguageToUserGroup(englishLanguage);
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.reloadPage();
  await umbracoUi.userGroup.doesUserGroupContainLanguage(englishLanguage);
  expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'en-US')).toBeTruthy();
})

test('can enable all languages for a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickAllowAccessToAllLanguages();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesUserGroupContainAccessToAllLanguages(userGroupName)).toBeTruthy();
})

test('can add multiple languages to a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createUserGroupWithLanguage(userGroupName, 'en-US');
  const danishLanguage = 'Danish';
  await umbracoApi.language.ensureNameNotExists(danishLanguage);
  await umbracoApi.language.createDanishLanguage();
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.addLanguageToUserGroup(danishLanguage);
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.reloadPage();
  await umbracoUi.userGroup.doesUserGroupContainLanguage(englishLanguage);
  await umbracoUi.userGroup.doesUserGroupContainLanguage(danishLanguage);
  expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'en-US')).toBeTruthy();
  expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'da')).toBeTruthy();

  // Clean
  await umbracoApi.language.ensureNameNotExists(danishLanguage);
})

test('can remove language from a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createUserGroupWithLanguage(userGroupName, 'en-US');
  expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'en-US')).toBeTruthy();
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickRemoveLanguageFromUserGroup(englishLanguage);
  await umbracoUi.userGroup.clickConfirmRemoveButton();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.reloadPage();
  await umbracoUi.userGroup.doesUserGroupContainLanguage(englishLanguage, false);
  expect(await umbracoApi.userGroup.doesUserGroupContainLanguage(userGroupName, 'en-US')).toBeFalsy();
})

test('can add a content start node to a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseContentStartNodeButton();
  await umbracoUi.userGroup.clickLabelWithName(documentName);
  await umbracoUi.userGroup.clickChooseContainerButton();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesUserGroupContainContentStartNodeId(userGroupName, documentId)).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can remove a content start node from a user group ', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.userGroup.createUserGroupWithDocumentStartNode(userGroupName, documentId);
  expect(await umbracoApi.userGroup.doesUserGroupContainContentStartNodeId(userGroupName, documentId)).toBeTruthy();
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickRemoveContentStartNodeFromUserGroup(documentName);
  await umbracoUi.userGroup.clickConfirmRemoveButton();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesUserGroupContainContentStartNodeId(userGroupName, documentId)).toBeFalsy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(documentTypeName);
});

test('can enable access to all content from a user group ', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickAllowAccessToAllDocuments();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesUserGroupContainDocumentRootAccess(userGroupName)).toBeTruthy();
});

test('can add a media start node to a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const mediaName = 'TestMedia';
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseMediaStartNodeButton();
  await umbracoUi.userGroup.selectMediaWithName(mediaName);
  await umbracoUi.userGroup.clickChooseModalButton();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesUserGroupContainMediaStartNodeId(userGroupName, mediaId)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can remove a media start node from a user group ', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaName = 'TestMedia';
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaName);
  await umbracoApi.userGroup.createUserGroupWithMediaStartNode(userGroupName, mediaId);
  expect(await umbracoApi.userGroup.doesUserGroupContainMediaStartNodeId(userGroupName, mediaId)).toBeTruthy();
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickRemoveMediaStartNodeFromUserGroup(mediaName);
  await umbracoUi.userGroup.clickConfirmRemoveButton();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesUserGroupContainMediaStartNodeId(userGroupName, mediaId)).toBeFalsy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can enable access to all media in a user group ', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickAllowAccessToAllMedia();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesUserGroupContainMediaRootAccess(userGroupName)).toBeTruthy();
});

test('can enable all permissions for a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
  await umbracoUi.userGroup.clickPermissionsByName(allPermissions.uiPermission);
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.reloadPage();
  await umbracoUi.userGroup.doesUserGroupHavePermissionEnabled(allPermissions.uiPermission);
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.fallbackPermissions).toEqual(allPermissions.verbPermission);
});

test('can add granular permission to a specific document for a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickAddGranularPermission();
  await umbracoUi.userGroup.clickLabelWithName(documentName);
  await umbracoUi.userGroup.clickGranularPermissionsByName([allPermissions.uiPermission[0]]);
  await umbracoUi.userGroup.clickConfirmButton();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesUserGroupContainGranularPermissionsForDocument(userGroupName, documentId, [allPermissions.verbPermission[0]])).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(documentTypeName);
});

test('can add all granular permissions to a specific document for a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickAddGranularPermission();
  await umbracoUi.userGroup.clickLabelWithName(documentName);
  await umbracoUi.userGroup.clickGranularPermissionsByName(allPermissions.uiPermission);
  await umbracoUi.userGroup.clickConfirmButton();
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.reloadPage();
  await umbracoUi.userGroup.clickGranularPermissionWithName(documentName);
  await umbracoUi.userGroup.doesUserGroupHavePermissionEnabled(allPermissions.uiPermission);
  expect(await umbracoApi.userGroup.doesUserGroupContainGranularPermissionsForDocument(userGroupName, documentId, allPermissions.verbPermission)).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(documentTypeName);
});

test('can remove granular permission to a specific document for a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.userGroup.createUserGroupWithPermissionsForSpecificDocumentWithBrowseNode(userGroupName, documentId);
  expect(await umbracoApi.userGroup.doesUserGroupContainGranularPermissionsForDocument(userGroupName, documentId, [allPermissions.verbPermission[0]])).toBeTruthy();
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickRemoveGranularPermissionWithName(documentName);
  await umbracoUi.userGroup.clickSaveButton();

  // Assert
  await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.userGroup.doesUserGroupContainGranularPermissionsForDocument(userGroupName, documentId, [allPermissions.verbPermission[0]])).toBeFalsy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(documentTypeName);
});
