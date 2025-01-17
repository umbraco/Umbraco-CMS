import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

const userGroupName = 'TestUserGroup';
let userGroupId = null;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test('can go to section defined in userGroup', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);

  // Act
  await umbracoUi.goToBackOffice();

  // Assert
  await umbracoUi.content.isSectionWithNameVisible(ConstantHelper.sections.content);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
});

test('can not see section that is not defined in userGroup', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);

  // Act
  await umbracoUi.goToBackOffice();

  // Assert
  await umbracoUi.content.isSectionWithNameVisible(ConstantHelper.sections.content);
  await umbracoUi.content.isSectionWithNameVisible(ConstantHelper.sections.media, false);
  await umbracoUi.content.isSectionWithNameVisible(ConstantHelper.sections.settings, false);
  await umbracoUi.content.isSectionWithNameVisible(ConstantHelper.sections.users, false);
  await umbracoUi.content.isSectionWithNameVisible(ConstantHelper.sections.members, false);
  await umbracoUi.content.isSectionWithNameVisible(ConstantHelper.sections.dictionary, false);
  await umbracoUi.content.isSectionWithNameVisible(ConstantHelper.sections.packages, false);
});
