import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.users);
  await umbracoUi.userGroup.clickUserGroupsButton();
});

test('the default configuration of Administrators is correct', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Administrators';
  const sections = [
    "Umb.Section.Content",
    "Umb.Section.Media",
    "Umb.Section.Members",
    "Umb.Section.Packages",
    "Umb.Section.Settings",
    "Umb.Section.Translation",
    "Umb.Section.Users",
    "Umb.Section.Library"
  ];
  const fallbackDocumentPermissions = [
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
    "Umb.Document.Unpublish",
    "Umb.Document.Read",
    "Umb.Document.CreateBlueprint",
    "Umb.Document.Notifications",
  ];
  const fallbackElementPermissions = [
    "Umb.Element.Create",
    "Umb.Element.Update",
    "Umb.Element.Delete",
    "Umb.Element.Move",
    "Umb.Element.Duplicate",
    "Umb.Element.Publish",
    "Umb.Element.Unpublish",
    "Umb.Element.Read",
    "Umb.Element.Rollback",
  ];
  const granularPermissions: any = [];
  const hasAccessToAllLanguages = true;
  const documentRootAccess = true;
  const mediaRootAccess = true;
  const uiDocumentPermissions = await umbracoApi.userGroup.convertApiDocumentPermissionsToUiDocumentPermissions(fallbackDocumentPermissions);
  const uiElementPermissions = await umbracoApi.userGroup.convertApiElementPermissionsToUiElementPermissions(fallbackElementPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Document permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Document Property Value permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesDocumentPermissionsSettingsHaveValue(ConstantHelper.userGroupDocumentPermissionsSettings);
  await umbracoUi.userGroup.doesElementPermissionsSettingsHaveValue(ConstantHelper.userGroupElementPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHaveDocumentPermissionEnabled(uiDocumentPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveElementPermissionEnabled(uiElementPermissions);
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

test('the default configuration of Editors is correct', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Editors';
  const sections = [
    "Umb.Section.Content",
    "Umb.Section.Media",
    "Umb.Section.Library"
  ];
  const fallbackDocumentPermissions = [
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
  const fallbackElementPermissions = [
    "Umb.Element.Create",
    "Umb.Element.Update",
    "Umb.Element.Delete",
    "Umb.Element.Move",
    "Umb.Element.Duplicate",
    "Umb.Element.Publish",
    "Umb.Element.Unpublish",
    "Umb.Element.Read",
    "Umb.Element.Rollback",
  ];
  const granularPermissions: string[] = [];
  const hasAccessToAllLanguages = true;
  const documentRootAccess = true;
  const mediaRootAccess = true;
  const uiDocumentPermissions = await umbracoApi.userGroup.convertApiDocumentPermissionsToUiDocumentPermissions(fallbackDocumentPermissions);
  const uiElementPermissions = await umbracoApi.userGroup.convertApiElementPermissionsToUiElementPermissions(fallbackElementPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
   await umbracoUi.userGroup.doesSettingHaveValue('Document permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Document Property Value permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesDocumentPermissionsSettingsHaveValue(ConstantHelper.userGroupDocumentPermissionsSettings);
  await umbracoUi.userGroup.doesElementPermissionsSettingsHaveValue(ConstantHelper.userGroupElementPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHaveDocumentPermissionEnabled(uiDocumentPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveElementPermissionEnabled(uiElementPermissions);
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

test('the default configuration of Sensitive data is correct', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Sensitive data';
  const sections: string[] = [];
  const fallbackDocumentPermissions: string[] = [];
  const fallbackElementPermissions: string[] = [];
  const granularPermissions: string[] = [];
  const hasAccessToAllLanguages = false;
  const documentRootAccess = false;
  const mediaRootAccess = false;
  const uiDocumentPermissions = await umbracoApi.userGroup.convertApiDocumentPermissionsToUiDocumentPermissions(fallbackDocumentPermissions);
  const uiElementPermissions = await umbracoApi.userGroup.convertApiElementPermissionsToUiElementPermissions(fallbackElementPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Document permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Document Property Value permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesDocumentPermissionsSettingsHaveValue(ConstantHelper.userGroupDocumentPermissionsSettings);
  await umbracoUi.userGroup.doesElementPermissionsSettingsHaveValue(ConstantHelper.userGroupElementPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHaveDocumentPermissionEnabled(uiDocumentPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveElementPermissionEnabled(uiElementPermissions);
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

test('the default configuration of Translators data is correct', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Translators';
  const sections = ["Umb.Section.Translation"];
  const fallbackDocumentPermissions = [
    "Umb.Document.Update",
    "Umb.Document.Read",
  ];
  const fallbackElementPermissions = [
    "Umb.Document.Update",
    "Umb.Element.Read",
  ];
  const granularPermissions: string[] = [];
  const hasAccessToAllLanguages = true;
  const documentRootAccess = true;
  const mediaRootAccess = true;
  const uiDocumentPermissions = await umbracoApi.userGroup.convertApiDocumentPermissionsToUiDocumentPermissions(fallbackDocumentPermissions);
  const uiElementPermissions = await umbracoApi.userGroup.convertApiElementPermissionsToUiElementPermissions(fallbackElementPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Document permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Document Property Value permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesDocumentPermissionsSettingsHaveValue(ConstantHelper.userGroupDocumentPermissionsSettings);
  await umbracoUi.userGroup.doesElementPermissionsSettingsHaveValue(ConstantHelper.userGroupElementPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHaveDocumentPermissionEnabled(uiDocumentPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveElementPermissionEnabled(uiElementPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveSections(uiSections);
  await umbracoUi.userGroup.doesUserGroupSectionsHaveCount(uiSections.length);
  expect(await umbracoApi.userGroup.doesUserGroupHaveSections(userGroupName, sections)).toBeTruthy();
  // Fixme - Uncomment this when the front-end is ready. Currently the fallbackPermissions includes some unnecessary values such as ":", "5", "T"
  // expect(await umbracoApi.userGroup.doesUserGroupHaveFallbackPermissions(userGroupName, fallbackPermissions)).toBeTruthy();
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.hasAccessToAllLanguages).toEqual(hasAccessToAllLanguages);
  expect(userGroupData.documentRootAccess).toEqual(documentRootAccess);
  expect(userGroupData.mediaRootAccess).toEqual(mediaRootAccess);
  expect(userGroupData.permissions).toEqual(granularPermissions);
});

test('the default configuration of Writers data is correct', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupName = 'Writers';
  const sections = ["Umb.Section.Content", "Umb.Section.Library"];
  const fallbackDocumentPermissions = [
    "Umb.Document.Create",
    "Umb.Document.Update",
    "Umb.Document.Read",
    "Umb.Document.Notifications",
  ];
  const fallbackElementPermissions = [
    "Umb.Element.Create",
    "Umb.Element.Update",
    "Umb.Element.Read",
  ];
  const granularPermissions: string[] = [];
  const hasAccessToAllLanguages = true;
  const documentRootAccess = true;
  const mediaRootAccess = true;
  const uiDocumentPermissions = await umbracoApi.userGroup.convertApiDocumentPermissionsToUiDocumentPermissions(fallbackDocumentPermissions);
  const uiElementPermissions = await umbracoApi.userGroup.convertApiElementPermissionsToUiElementPermissions(fallbackElementPermissions);
  const uiSections = await umbracoApi.userGroup.convertApiSectionsToUiSections(sections);

  // Act 
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.doesSettingHaveValue('Assign access', ConstantHelper.userGroupAssignAccessSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Document permissions', ConstantHelper.userGroupDefaultPermissionsSettings);
  await umbracoUi.userGroup.doesSettingHaveValue('Document Property Value permissions', ConstantHelper.userGroupGranularPermissionsSettings);
  await umbracoUi.userGroup.doesDocumentPermissionsSettingsHaveValue(ConstantHelper.userGroupDocumentPermissionsSettings);
  await umbracoUi.userGroup.doesElementPermissionsSettingsHaveValue(ConstantHelper.userGroupElementPermissionsSettings);
  await umbracoUi.userGroup.doesUserGroupHaveDocumentPermissionEnabled(uiDocumentPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveElementPermissionEnabled(uiElementPermissions);
  await umbracoUi.userGroup.doesUserGroupHaveSections(uiSections);
  await umbracoUi.userGroup.doesUserGroupSectionsHaveCount(uiSections.length);
  expect(await umbracoApi.userGroup.doesUserGroupHaveSections(userGroupName, sections)).toBeTruthy();
  // Fixme - Uncomment this when the front-end is ready. Currently the fallbackPermissions includes some unnecessary values such as ":", "5", "T"
  // expect(await umbracoApi.userGroup.doesUserGroupHaveFallbackPermissions(userGroupName, fallbackPermissions)).toBeTruthy();
  const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(userGroupData.hasAccessToAllLanguages).toEqual(hasAccessToAllLanguages);
  expect(userGroupData.documentRootAccess).toEqual(documentRootAccess);
  expect(userGroupData.mediaRootAccess).toEqual(mediaRootAccess);
  expect(userGroupData.permissions).toEqual(granularPermissions);
});