import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

const userGroupName = 'TestUserGroup';
let userGroupId = null;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  userGroupId = await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test('can see correct translation for content in english', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [], true, [], false, 'en-us');
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);

  // Act
  await umbracoUi.goToBackOffice();

  // Assert
  await umbracoUi.user.isSectionWithNameVisible(ConstantHelper.sections.content, true);
});

test('can see correct translation for content in danish', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [], true, [], false, 'da-dk');
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);

  // Act
  await umbracoUi.goToBackOffice();

  // Assert
  // Indhold is the Danish translation of Content
  await umbracoUi.user.isSectionWithNameVisible('Indhold', true);
});
