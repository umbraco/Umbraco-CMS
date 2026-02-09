import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

const userGroupName = 'TestUserGroup';
let userGroupId = null;

let memberId = '';
let memberTypeId = '';
const memberName = 'Test Member';
const memberTypeName = 'Test Member Type';
const comment = 'This is test comment';
const username = 'testmember';
const email = 'testmember@acceptance.test';
const password = '0123456789';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoApi.member.ensureNameNotExists(memberName);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoApi.member.ensureNameNotExists(memberName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test('can access members section with section enabled', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMemberSection(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [], true, [], false, 'en-us');
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.member.goToSection(ConstantHelper.sections.members, false);

  // Assert
  await umbracoUi.user.isSectionWithNameVisible(ConstantHelper.sections.content, false);
  await umbracoUi.member.doesErrorNotificationHaveText(NotificationConstantHelper.error.noAccessToResource, false);
});

test('can create member with members section set', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMemberSection(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [], true, [], false, 'en-us');
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.member.goToSection(ConstantHelper.sections.members, false);
  await umbracoUi.member.clickMembersMenu();

  // Act
  await umbracoUi.member.clickCreateMembersButton();
  await umbracoUi.member.enterMemberName(memberName);
  await umbracoUi.member.clickInfoTab();
  await umbracoUi.member.enterUsername(username);
  await umbracoUi.member.enterEmail(email);
  await umbracoUi.member.enterPassword(password);
  await umbracoUi.member.enterConfirmPassword(password);
  await umbracoUi.member.clickDetailsTab();
  await umbracoUi.member.enterComments(comment);
  await umbracoUi.member.clickSaveButtonAndWaitForMemberToBeCreated();

  // Assert
  await umbracoUi.member.doesErrorNotificationHaveText(NotificationConstantHelper.error.noAccessToResource, false);
});

test('can update member with members section set', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithMemberSection(userGroupName);
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  const updatedUsername = 'updatedusername';
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [], true, [], false, 'en-us');
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.member.goToSection(ConstantHelper.sections.members, false);
  await umbracoUi.member.clickMembersMenu();

  // Act
  await umbracoUi.member.clickMemberLinkByName(memberName);
  await umbracoUi.member.enterUsername(updatedUsername);
  await umbracoUi.member.clickSaveButtonAndWaitForMemberToBeUpdated();

  // Assert
  await umbracoUi.member.doesErrorNotificationHaveText(NotificationConstantHelper.error.noAccessToResource, false);
  const memberData = await umbracoApi.member.get(memberId);
  expect(memberData.username).toBe(updatedUsername);
});
