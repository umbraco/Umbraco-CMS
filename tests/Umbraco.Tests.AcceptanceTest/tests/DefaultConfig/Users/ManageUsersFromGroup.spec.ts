import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const userGroupName = 'TestUserGroup';
const writersUserGroupName = 'Writers';
const userA = {name: 'Manage Users User A', email: 'manageusersa@acceptance.test'};
const userB = {name: 'Manage Users User B', email: 'manageusersb@acceptance.test'};
const userC = {name: 'Manage Users User C', email: 'manageusersc@acceptance.test'};

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.user.ensureNameNotExists(userA.name);
  await umbracoApi.user.ensureNameNotExists(userB.name);
  await umbracoApi.user.ensureNameNotExists(userC.name);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.users);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.user.ensureNameNotExists(userA.name);
  await umbracoApi.user.ensureNameNotExists(userB.name);
  await umbracoApi.user.ensureNameNotExists(userC.name);
});

test('can view existing users in a user group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [userGroupId]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [userGroupId]);

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Assert
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userA.name);
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userB.name);
});

test('cannot see any users in an empty user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Assert
  expect(await umbracoUi.userGroup.getUsersInGroupCount()).toBe(0);
});

test('can add a user to an existing user group from the workspace', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const seedUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [seedUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickChooseModalButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Assert
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userA.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [seedUserGroup.id, userGroupId])).toBeTruthy();
});

test('can add multiple users to an existing user group at once', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const seedUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [seedUserGroup.id]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [seedUserGroup.id]);
  await umbracoApi.user.createDefaultUser(userC.name, userC.email, [seedUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickUserCardWithName(userB.name);
  await umbracoUi.userGroup.clickUserCardWithName(userC.name);
  await umbracoUi.userGroup.clickChooseModalButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Assert
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userA.name);
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userB.name);
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userC.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [seedUserGroup.id, userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userB.name, [seedUserGroup.id, userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userC.name, [seedUserGroup.id, userGroupId])).toBeTruthy();
});

test('can add a new user via the picker without removing existing users', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const seedUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [userGroupId]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [userGroupId]);
  await umbracoApi.user.createDefaultUser(userC.name, userC.email, [seedUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userC.name);
  await umbracoUi.userGroup.clickChooseModalButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Assert
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userA.name);
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userB.name);
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userC.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userB.name, [userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userC.name, [seedUserGroup.id, userGroupId])).toBeTruthy();
});

test('can remove a user from a user group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [userGroupId]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [userGroupId]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickRemoveButtonForUserWithName(userA.name);
  await umbracoUi.userGroup.clickConfirmRemoveButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Assert
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userA.name, false);
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userB.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [userGroupId])).toBeFalsy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userB.name, [userGroupId])).toBeTruthy();
});

test('can remove all users from a user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [userGroupId]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [userGroupId]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickRemoveButtonForUserWithName(userA.name);
  await umbracoUi.userGroup.clickConfirmRemoveButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.userGroup.clickRemoveButtonForUserWithName(userB.name);
  await umbracoUi.userGroup.clickConfirmRemoveButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Assert
  expect(await umbracoUi.userGroup.getUsersInGroupCount()).toBe(0);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [userGroupId])).toBeFalsy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userB.name, [userGroupId])).toBeFalsy();
});

test('can add and then remove the same user in the same session', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const seedUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [seedUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickChooseModalButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.userGroup.clickRemoveButtonForUserWithName(userA.name);
  await umbracoUi.userGroup.clickConfirmRemoveButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Assert
  expect(await umbracoUi.userGroup.getUsersInGroupCount()).toBe(0);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [userGroupId])).toBeFalsy();
});

test('can persist user additions after navigating away from and returning to the user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const seedUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [seedUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickChooseModalButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Assert
  await umbracoUi.userGroup.isUserVisibleInGroupUsers(userA.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [seedUserGroup.id, userGroupId])).toBeTruthy();
});

test('can add users while creating a new user group and saving persists them', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const seedUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [seedUserGroup.id]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [seedUserGroup.id]);

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickCreateLink();
  await umbracoUi.userGroup.enterUserGroupName(userGroupName);
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickUserCardWithName(userB.name);
  await umbracoUi.userGroup.clickChooseModalButton();
  const userGroupId = await umbracoUi.userGroup.clickSaveButtonAndWaitForUserGroupToBeCreated();
  // Wait for the deferred user persistence to fire after the group transitions from new to persisted.
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

  // Assert
  expect(await umbracoApi.userGroup.doesExist(userGroupId)).toBe(true);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [seedUserGroup.id, userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userB.name, [seedUserGroup.id, userGroupId])).toBeTruthy();
});
