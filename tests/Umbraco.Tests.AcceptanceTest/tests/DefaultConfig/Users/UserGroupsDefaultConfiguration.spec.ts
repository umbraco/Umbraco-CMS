import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.users);
  await umbracoUi.userGroup.clickUserGroupsButton();
});

test('the default configuration of Administrators is correct', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Administrators';
  const sections = [
    "Umb.Section.Content",
    "Umb.Section.Media",
    "Umb.Section.Members",
    "Umb.Section.Packages",
    "Umb.Section.Settings",
    "Umb.Section.Translation",
    "Umb.Section.Users"
  ];
  const fallbackPermissions = [
    "Umb.Document.Create",
    "Umb.Document.Update",
    "Umb.Document.Delete",
    "Umb.Document.Move",
    "Umb.Document.Duplicate",
    "Umb.Document.Sort",
    "Umb.Document.Rollback",
    "Umb.Document.PublicAccess",
    "Umb.Document.CultureAndHostnames",
    "Umb.Document.Publish",
    "Umb.Document.Permissions",
    "Umb.Document.Unpublish",
    "Umb.Document.Read",
    "Umb.Document.CreateBlueprint",
    "Umb.Document.Notifications",
  ];
  const granularPermissions = [];
  const hasAccessToAllLanguages = true;
  const documentRootAccess = true;
  const mediaRootAccess = true;
  const uiPermissions = await umbracoApi.userGroup.convertApiPermissionsToUiPermissions(fallbackPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Default permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Granular permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesPermissionsSettingsHaveValue(ConstantHelper.userGroupPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHavePermissionEnabled(uiPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveSections(uiSections);
  await umbracoUi.userGroup.doesUserGroupSectionsHaveCount(uiSections.length);
  // Fixme - Uncomment this when the front-end is ready. Currently the sections includes "Umb.Section.Forms" which should be removed.
  //expect(await umbracoApi.userGroup.doesUserGroupHaveSections(userGroupName, sections)).toBeTruthy();
  // Fixme - Uncomment this when the front-end is ready. Currently the fallbackPermissions includes some unnecessary values such as ":", "5", "T"
  //expect(await umbracoApi.userGroup.doesUserGroupHaveFallbackPermissions(userGroupName, fallbackPermissions)).toBeTruthy();
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.hasAccessToAllLanguages).toEqual(hasAccessToAllLanguages);
  expect(userGroupData.documentRootAccess).toEqual(documentRootAccess);
  expect(userGroupData.mediaRootAccess).toEqual(mediaRootAccess);
  expect(userGroupData.permissions).toEqual(granularPermissions);
});

test('the default configuration of Editors is correct', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Editors';
  const sections = [
    "Umb.Section.Content",
    "Umb.Section.Media"
  ];
  const fallbackPermissions = [
    "Umb.Document.Create",
    "Umb.Document.Update",
    "Umb.Document.Delete",
    "Umb.Document.Move",
    "Umb.Document.Duplicate",
    "Umb.Document.Sort",
    "Umb.Document.Rollback",
    "Umb.Document.PublicAccess",
    "Umb.Document.Publish",
    "Umb.Document.Unpublish",
    "Umb.Document.Read",
    "Umb.Document.CreateBlueprint",
    "Umb.Document.Notifications",
  ];
  const granularPermissions = [];
  const hasAccessToAllLanguages = true;
  const documentRootAccess = true;
  const mediaRootAccess = true;
  const uiPermissions = await umbracoApi.userGroup.convertApiPermissionsToUiPermissions(fallbackPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Default permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Granular permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesPermissionsSettingsHaveValue(ConstantHelper.userGroupPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHavePermissionEnabled(uiPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveSections(uiSections);
  await umbracoUi.userGroup.doesUserGroupSectionsHaveCount(uiSections.length);
  // Fixme - Uncomment this when the front-end is ready. Currently the sections includes "Umb.Section.Forms" which should be removed.
  //expect(await umbracoApi.userGroup.doesUserGroupHaveSections(userGroupName, sections)).toBeTruthy();
  // Fixme - Uncomment this when the front-end is ready. Currently the fallbackPermissions includes some unnecessary values such as ":", "5", "T"
  //expect(await umbracoApi.userGroup.doesUserGroupHaveFallbackPermissions(userGroupName, fallbackPermissions)).toBeTruthy();
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.hasAccessToAllLanguages).toEqual(hasAccessToAllLanguages);
  expect(userGroupData.documentRootAccess).toEqual(documentRootAccess);
  expect(userGroupData.mediaRootAccess).toEqual(mediaRootAccess);
  expect(userGroupData.permissions).toEqual(granularPermissions);
});

test('the default configuration of Sensitive data is correct', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Sensitive data';
  const sections = [];
  const fallbackPermissions = [];
  const granularPermissions = [];
  const hasAccessToAllLanguages = false;
  const documentRootAccess = false;
  const mediaRootAccess = false;
  const uiPermissions = await umbracoApi.userGroup.convertApiPermissionsToUiPermissions(fallbackPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Default permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Granular permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesPermissionsSettingsHaveValue(ConstantHelper.userGroupPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHavePermissionEnabled(uiPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveSections(uiSections);
  await umbracoUi.userGroup.doesUserGroupSectionsHaveCount(uiSections.length);
  // Fixme - Uncomment this when the front-end is ready. Currently the sections includes "Umb.Section.Forms" which should be removed.
  //expect(await umbracoApi.userGroup.doesUserGroupHaveSections(userGroupName, sections)).toBeTruthy();
  // Fixme - Uncomment this when the front-end is ready. Currently the fallbackPermissions includes some unnecessary values such as ":", "5", "T"
  //expect(await umbracoApi.userGroup.doesUserGroupHaveFallbackPermissions(userGroupName, fallbackPermissions)).toBeTruthy();
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.hasAccessToAllLanguages).toEqual(hasAccessToAllLanguages);
  expect(userGroupData.documentRootAccess).toEqual(documentRootAccess);
  expect(userGroupData.mediaRootAccess).toEqual(mediaRootAccess);
  expect(userGroupData.permissions).toEqual(granularPermissions);
});

test('the default configuration of Translators data is correct', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Translators';
  const sections = ["Umb.Section.Translation"];
  const fallbackPermissions = [
    "Umb.Document.Update",
    "Umb.Document.Read"
  ];
  const granularPermissions = [];
  const hasAccessToAllLanguages = true;
  const documentRootAccess = true;
  const mediaRootAccess = true;
  const uiPermissions = await umbracoApi.userGroup.convertApiPermissionsToUiPermissions(fallbackPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Default permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Granular permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesPermissionsSettingsHaveValue(ConstantHelper.userGroupPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHavePermissionEnabled(uiPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveSections(uiSections);
  await umbracoUi.userGroup.doesUserGroupSectionsHaveCount(uiSections.length);
  expect(await umbracoApi.userGroup.doesUserGroupHaveSections(userGroupName, sections)).toBeTruthy();
  expect(await umbracoApi.userGroup.doesUserGroupHaveFallbackPermissions(userGroupName, fallbackPermissions)).toBeTruthy();
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.hasAccessToAllLanguages).toEqual(hasAccessToAllLanguages);
  expect(userGroupData.documentRootAccess).toEqual(documentRootAccess);
  expect(userGroupData.mediaRootAccess).toEqual(mediaRootAccess);
  expect(userGroupData.permissions).toEqual(granularPermissions);
});

test('the default configuration of Writers data is correct', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Writers';
  const sections = ["Umb.Section.Content"];
  const fallbackPermissions = [
    "Umb.Document.Create",
    "Umb.Document.Update",
    "Umb.Document.Read",
    "Umb.Document.Notifications"
  ];
  const granularPermissions = [];
  const hasAccessToAllLanguages = true;
  const documentRootAccess = true;
  const mediaRootAccess = true;
  const uiPermissions = await umbracoApi.userGroup.convertApiPermissionsToUiPermissions(fallbackPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Default permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Granular permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesPermissionsSettingsHaveValue(ConstantHelper.userGroupPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHavePermissionEnabled(uiPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveSections(uiSections);
  await umbracoUi.userGroup.doesUserGroupSectionsHaveCount(uiSections.length);
  expect(await umbracoApi.userGroup.doesUserGroupHaveSections(userGroupName, sections)).toBeTruthy();
  // Fixme - Uncomment this when the front-end is ready. Currently the fallbackPermissions includes some unnecessary values such as ":", "5", "T"
  //expect(await umbracoApi.userGroup.doesUserGroupHaveFallbackPermissions(userGroupName, fallbackPermissions)).toBeTruthy();
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.hasAccessToAllLanguages).toEqual(hasAccessToAllLanguages);
  expect(userGroupData.documentRootAccess).toEqual(documentRootAccess);
  expect(userGroupData.mediaRootAccess).toEqual(mediaRootAccess);
  expect(userGroupData.permissions).toEqual(granularPermissions);
});