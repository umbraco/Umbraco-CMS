import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let memberId = '';
let memberTypeId = '';
const defaultMemberTypeName = 'Member';
const memberName = 'Test Member';
const memberTypeName = 'Test Member Type';
const comment = 'This is test comment';
const username = 'testmember';
const email = 'testmember@acceptance.test';
const password = '0123456789';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoApi.member.ensureNameNotExists(memberName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoApi.member.ensureNameNotExists(memberName);
});

test('can create a member', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickCreateButton();
  await umbracoUi.member.enterMemberName(memberName);
  await umbracoUi.member.enterComments(comment);
  await umbracoUi.member.clickInfoTab();
  await umbracoUi.member.enterUsername(username);
  await umbracoUi.member.enterEmail(email);
  await umbracoUi.member.enterPassword(password);
  await umbracoUi.member.enterConfirmPassword(password);
  await umbracoUi.member.clickSaveButton();

  // Assert
  //await umbracoUi.member.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created)
  await umbracoUi.member.isErrorNotificationVisible(false);;
  expect(await umbracoApi.member.doesNameExist(memberName)).toBeTruthy();
});

test('can edit comments', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const defaultMemberTypeData = await umbracoApi.memberType.getByName(defaultMemberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, defaultMemberTypeData.id, email, username, password);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.member.enterComments(comment);
  await umbracoUi.member.clickSaveButton();

  // Assert
  //await umbracoUi.member.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved)
  await umbracoUi.member.isErrorNotificationVisible(false);;
  const memberData = await umbracoApi.member.get(memberId);
  expect(memberData.values[0].value).toBe(comment);
});

test('can edit username', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedUsername = 'updatedusername';
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.member.enterUsername(updatedUsername);
  await umbracoUi.member.clickSaveButton();

  // Assert
  //await umbracoUi.member.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved)
  await umbracoUi.member.isErrorNotificationVisible(false);;
  const memberData = await umbracoApi.member.get(memberId);
  expect(memberData.username).toBe(updatedUsername);
});

test('can edit email', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedEmail = 'updated@acceptance.test';
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.member.enterEmail(updatedEmail);
  await umbracoUi.member.clickSaveButton();

  // Assert
  //await umbracoUi.member.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved)
  await umbracoUi.member.isErrorNotificationVisible(false);;
  const memberData = await umbracoApi.member.get(memberId);
  expect(memberData.email).toBe(updatedEmail);
});

test('can edit password', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedPassword = '9876543210';
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.member.clickChangePasswordButton();
  await umbracoUi.member.enterNewPassword(updatedPassword);
  await umbracoUi.member.enterConfirmNewPassword(updatedPassword);
  await umbracoUi.member.clickSaveButton();

  // Assert
  //await umbracoUi.member.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved)
  await umbracoUi.member.isErrorNotificationVisible(false);;
});

test('can add member group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const memberGroupName = 'TestMemberGroup';
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
  const memberGroupId = await umbracoApi.memberGroup.create(memberGroupName);
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.member.chooseMemberGroup(memberGroupName);
  await umbracoUi.member.clickSaveButton();

  // Assert
  //await umbracoUi.member.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved)
  await umbracoUi.member.isErrorNotificationVisible(false);;
  const memberData = await umbracoApi.member.get(memberId);
  expect(memberData.groups[0]).toBe(memberGroupId);

  // Clean
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
});

test('can remove member group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const memberGroupName = 'TestMemberGroup';
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
  const memberGroupId = await umbracoApi.memberGroup.create(memberGroupName);
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createMemberWithMemberGroup(memberName, memberTypeId, email, username, password, memberGroupId);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.member.clickRemoveMemberGroupByName(memberGroupName);
  await umbracoUi.member.clickConfirmRemoveButton();
  await umbracoUi.member.clickSaveButton();

  // Assert
  //await umbracoUi.member.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved)
  await umbracoUi.member.isErrorNotificationVisible(false);;
  const memberData = await umbracoApi.member.get(memberId);
  expect(memberData.groups.length).toBe(0);

  // Clean
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
});

test('can view member info', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);

  // Assert
  const memberData = await umbracoApi.member.get(memberId);
  await umbracoUi.member.doesMemberInfoHaveValue('Failed login attempts', memberData.failedPasswordAttempts.toString());
  await umbracoUi.member.doesMemberInfoHaveValue('Last lockout date', memberData.lastLoginDate == null ? 'Never' : memberData.lastLoginDate);
  await umbracoUi.member.doesMemberInfoHaveValue('Last login', memberData.lastLoginDate == null ? 'Never' : memberData.lastLoginDate);
  await umbracoUi.member.doesMemberInfoHaveValue('Password changed', new Date(memberData.lastPasswordChangeDate).toLocaleString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "numeric",
    minute: "numeric",
    hour12: true,
  }));
});

test('can enable approved', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.member.clickApprovedToggle();
  await umbracoUi.member.clickSaveButton();

  // Assert
  //await umbracoUi.member.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved)
  await umbracoUi.member.isErrorNotificationVisible(false);;
  const memberData = await umbracoApi.member.get(memberId);
  expect(memberData.isApproved).toBe(true);
});

test('can delete member', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.memberGroup.clickActionsButton();
  await umbracoUi.memberGroup.clickDeleteButton();
  await umbracoUi.memberGroup.clickConfirmToDeleteButton();

  // Assert
  //await umbracoUi.member.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted)
  await umbracoUi.member.isErrorNotificationVisible(false);;
  expect(await umbracoApi.member.doesNameExist(memberName)).toBeFalsy();
});

test('cannot create member with invalid email', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const invalidEmail = 'invalidemail';
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickCreateButton();
  await umbracoUi.member.enterMemberName(memberName);
  await umbracoUi.member.enterComments(comment);
  await umbracoUi.member.clickInfoTab();
  await umbracoUi.member.enterUsername(username);
  await umbracoUi.member.enterEmail(invalidEmail);
  await umbracoUi.member.enterPassword(password);
  await umbracoUi.member.enterConfirmPassword(password);
  await umbracoUi.member.clickSaveButton();

  // Assert
  await umbracoUi.member.doesErrorNotificationHaveText(NotificationConstantHelper.error.invalidEmail);
  expect(await umbracoApi.member.doesNameExist(memberName)).toBeFalsy();
});

// TODO: Remove skip when the front-end is ready. Currently it is possible to update member with invalid email.
test.skip('cannot update email to an invalid email', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const invalidEmail = 'invalidemail';
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  await umbracoUi.member.goToMembers();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.member.enterEmail(invalidEmail);
  await umbracoUi.member.clickSaveButton();

  // Assert
  await umbracoUi.member.isErrorNotificationVisible();
  const memberData = await umbracoApi.member.get(memberId);
  expect(memberData.email).toBe(email);
});
