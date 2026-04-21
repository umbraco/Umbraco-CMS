import {expect} from "@playwright/test";
import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const memberTypeName = 'TestMemberType';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
});

test('can create a member type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.memberType.clickActionsMenuAtRoot();
  await umbracoUi.memberType.clickCreateActionMenuOption();
  await umbracoUi.memberType.clickMemberTypeButton();
  await umbracoUi.memberType.enterMemberTypeName(memberTypeName);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeCreated();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeTruthy();
  await umbracoUi.memberType.isMemberTypeTreeItemVisible(memberTypeName, true);
});

test('can rename a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'NotAMemberTypeName';
  await umbracoApi.memberType.ensureNameNotExists(wrongName);
  await umbracoApi.memberType.createDefaultMemberType(wrongName);

  // Act
  await umbracoUi.memberType.goToMemberType(wrongName);
  await umbracoUi.memberType.enterMemberTypeName(memberTypeName);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeTruthy();
  await umbracoUi.memberType.isMemberTypeTreeItemVisible(wrongName, false);
  await umbracoUi.memberType.isMemberTypeTreeItemVisible(memberTypeName, true);
});

test('can update the alias for a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const oldAlias = AliasHelper.toAlias(memberTypeName);
  const updatedAlias = 'TestMemberAlias';
  await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  const memberTypeDataOld = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeDataOld.alias).toBe(oldAlias);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.memberType.enterAliasName(updatedAlias);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.alias).toBe(updatedAlias);
});

test('can add an icon for a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const bugIcon = 'icon-bug';
  await umbracoApi.memberType.createDefaultMemberType(memberTypeName);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.memberType.updateIcon(bugIcon);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.icon).toBe(bugIcon);
  await umbracoUi.memberType.isTreeItemVisible(memberTypeName, true);
});

test('can delete a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.memberType.createDefaultMemberType(memberTypeName);

  // Act
  await umbracoUi.memberType.clickRootFolderCaretButton();
  await umbracoUi.memberType.clickActionsMenuForName(memberTypeName);
  await umbracoUi.memberType.clickDeleteAndConfirmButtonAndWaitForMemberTypeToBeDeleted();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeFalsy();
  await umbracoUi.memberType.isMemberTypeTreeItemVisible(memberTypeName, false);
});
