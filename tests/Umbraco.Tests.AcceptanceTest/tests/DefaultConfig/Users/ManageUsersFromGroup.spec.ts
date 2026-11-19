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

test('can view users in a user group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [userGroupId]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [userGroupId]);

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Assert
  // The workspace user list loads async after the group opens; poll so the count isn't read before it populates.
  await expect.poll(() => umbracoUi.userGroup.getUsersInGroupCount()).toBe(2);
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userA.name);
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userB.name);
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

test('can add a user to a user group from the workspace', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const writersUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [writersUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickChooseModalButtonAndWaitForGroupUsersUpdate();

  // Assert
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userA.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [writersUserGroup.id, userGroupId])).toBeTruthy();
});

test('can add multiple users to a user group at once', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const writersUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [writersUserGroup.id]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [writersUserGroup.id]);
  await umbracoApi.user.createDefaultUser(userC.name, userC.email, [writersUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickUserCardWithName(userB.name);
  await umbracoUi.userGroup.clickUserCardWithName(userC.name);
  await umbracoUi.userGroup.clickChooseModalButtonAndWaitForGroupUsersUpdate();

  // Assert
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userA.name);
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userB.name);
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userC.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [writersUserGroup.id, userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userB.name, [writersUserGroup.id, userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userC.name, [writersUserGroup.id, userGroupId])).toBeTruthy();
});

test('can add a new user via the picker without removing existing users', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const writersUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [userGroupId]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [userGroupId]);
  await umbracoApi.user.createDefaultUser(userC.name, userC.email, [writersUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userC.name);
  await umbracoUi.userGroup.clickChooseModalButtonAndWaitForGroupUsersUpdate();

  // Assert
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userA.name);
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userB.name);
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userC.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userB.name, [userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userC.name, [writersUserGroup.id, userGroupId])).toBeTruthy();
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
  await umbracoUi.userGroup.clickConfirmRemoveButtonAndWaitForGroupUsersUpdate();

  // Assert
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userA.name, false);
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userB.name);
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
  await umbracoUi.userGroup.clickConfirmRemoveButtonAndWaitForGroupUsersUpdate();
  await umbracoUi.userGroup.clickRemoveButtonForUserWithName(userB.name);
  await umbracoUi.userGroup.clickConfirmRemoveButtonAndWaitForGroupUsersUpdate();

  // Assert
  expect(await umbracoUi.userGroup.getUsersInGroupCount()).toBe(0);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [userGroupId])).toBeFalsy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userB.name, [userGroupId])).toBeFalsy();
});

test('can add and then remove the same user in the same session', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const writersUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [writersUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Act
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickChooseModalButtonAndWaitForGroupUsersUpdate();
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userA.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [writersUserGroup.id, userGroupId])).toBeTruthy();
  await umbracoUi.userGroup.clickRemoveButtonForUserWithName(userA.name);
  await umbracoUi.userGroup.clickConfirmRemoveButtonAndWaitForGroupUsersUpdate();

  // Assert
  expect(await umbracoUi.userGroup.getUsersInGroupCount()).toBe(0);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [userGroupId])).toBeFalsy();
});

test('can see user updates are persisted after navigating away from and returning to the user group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createEmptyUserGroup(userGroupName);
  const writersUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [writersUserGroup.id]);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickChooseModalButtonAndWaitForGroupUsersUpdate();

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);

  // Assert
  await umbracoUi.userGroup.isUserVisibleInUserGroup(userA.name);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [writersUserGroup.id, userGroupId])).toBeTruthy();
});

test('can add users while creating a new user group', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const writersUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  await umbracoApi.user.createDefaultUser(userA.name, userA.email, [writersUserGroup.id]);
  await umbracoApi.user.createDefaultUser(userB.name, userB.email, [writersUserGroup.id]);

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickCreateLink();
  await umbracoUi.userGroup.enterUserGroupName(userGroupName);
  await umbracoUi.userGroup.clickChooseUserButton();
  await umbracoUi.userGroup.clickUserCardWithName(userA.name);
  await umbracoUi.userGroup.clickUserCardWithName(userB.name);
  await umbracoUi.userGroup.clickChooseModalButton();
  const [, userGroupId] = await umbracoUi.userGroup.clickSaveButtonAndWaitForUserGroupWithUsersToBeCreated();

  // Assert
  expect(await umbracoApi.userGroup.doesExist(userGroupId)).toBe(true);
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userA.name, [writersUserGroup.id, userGroupId])).toBeTruthy();
  expect(await umbracoApi.user.doesUserContainUserGroupIds(userB.name, [writersUserGroup.id, userGroupId])).toBeTruthy();
});
