import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const testUser = ConstantHelper.testUserCredentials;

const userGroupName = 'TestUserGroup';

const refusedUserGroupName = 'TestRefusedUserGroup';
const targetUserName = 'TestTargetUser';
const targetUserEmail = 'TestTargetUser@EmailTest.test';
const writersUserGroupName = 'Writers';
const editorsUserGroupName = 'Editors';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.user.ensureNameNotExists(targetUserName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.userGroup.ensureNameNotExists(refusedUserGroupName);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(targetUserName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.userGroup.ensureNameNotExists(refusedUserGroupName);
});

test('cannot grant a user group a section the current user cannot access', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createUserGroupWithUsersSection(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.users, false);
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickCreateLink();
  await umbracoUi.userGroup.enterUserGroupName(refusedUserGroupName);
  await umbracoUi.userGroup.addSectionWithNameToUserGroup('Content');

  // Act
  await umbracoUi.userGroup.waitForResponseAfterExecutingPromise(
    ConstantHelper.apiEndpoints.userGroup,
    () => umbracoUi.userGroup.clickSaveButton(),
    ConstantHelper.statusCodes.forbidden,
    ConstantHelper.httpMethods.post);

  // Assert
  await umbracoUi.userGroup.isFailedStateButtonVisible();
  await umbracoApi.loginToAdminUser();
  expect(await umbracoApi.userGroup.doesNameExist(refusedUserGroupName)).toBeFalsy();
});

test('cannot assign a user to a user group the current user is not in', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const writersUserGroup = await umbracoApi.userGroup.getByName(writersUserGroupName);
  const editorsUserGroup = await umbracoApi.userGroup.getByName(editorsUserGroupName);
  await umbracoApi.user.createDefaultUser(targetUserName, targetUserEmail, [writersUserGroup.id]);
  const userGroupId = await umbracoApi.userGroup.createUserGroupWithUsersSection(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.user.goToSection(ConstantHelper.sections.users, false);
  await umbracoUi.user.clickUsersMenu();
  await umbracoUi.user.searchInUserSection(targetUserName);
  await umbracoUi.user.clickUserWithName(targetUserName);
  // The user group picker only offers groups the current user is in, so the request is changed to simulate a client that sends another group.
  await umbracoUi.page.route(`**${ConstantHelper.apiEndpoints.user}/*`, async (route) => {
    if (route.request().method() !== ConstantHelper.httpMethods.put) {
      return route.fallback();
    }
    const requestBody = route.request().postDataJSON();
    requestBody.userGroupIds = [...requestBody.userGroupIds, {id: editorsUserGroup.id}];
    await route.continue({postData: JSON.stringify(requestBody)});
  });

  // Act
  await umbracoUi.user.waitForResponseAfterExecutingPromise(
    ConstantHelper.apiEndpoints.user,
    () => umbracoUi.user.clickSaveButton(),
    ConstantHelper.statusCodes.forbidden,
    ConstantHelper.httpMethods.put);

  // Assert
  await umbracoUi.user.isFailedStateButtonVisible();
  await umbracoApi.loginToAdminUser();
  const targetUserData = await umbracoApi.user.getByName(targetUserName);
  expect(targetUserData.userGroupIds.map((userGroup) => userGroup.id)).not.toContain(editorsUserGroup.id);
});
