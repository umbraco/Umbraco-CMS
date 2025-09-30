import {NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const nameOfTheUser = 'TestUser';
const userEmail = 'TestUser@EmailTest.test';
const defaultUserGroupName = 'Writers';
let userCount = null;

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoApi.user.ensureNameNotExists(nameOfTheUser);
});

test.afterEach(async ({umbracoApi, umbracoUi}) => {
  // Waits so we can try to avoid db locks
  await umbracoUi.waitForTimeout(500);
  await umbracoApi.user.ensureNameNotExists(nameOfTheUser);
});

test('admin user can change own password', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userPassword = 'TestPassword';
  const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
  const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, [userGroup.id]);
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickUserWithName(nameOfTheUser);
  await umbracoUi.user.clickActionButton();
  await umbracoUi.user.clickChangePasswordButton();
  await umbracoUi.user.updatePassword(userPassword);

  // Assert
  await umbracoUi.user.isPasswordUpdatedForUserWithId(userId);
});

test('non-admin can change own password', async ({umbracoApi, umbracoUi}) => {
  // Arrange

  // Act

  // Assert

});
