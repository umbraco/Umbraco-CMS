import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const testUserName = 'Test Login User';
const testUserEmail = 'testloginuser@acceptance.test';
const testUserPassword = 'TestLoginUser123!';
const userGroupName = 'Writers';
const wrongPassword = 'WrongPassword123!';
const unknownEmail = 'does-not-exist@acceptance.test';

const adminEmail = process.env.UMBRACO_USER_LOGIN;
const adminPassword = process.env.UMBRACO_USER_PASSWORD;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.user.ensureNameNotExists(testUserName);
  await umbracoApi.resetAuthState();
  await umbracoUi.goToBackOffice();
  await umbracoUi.login.isLoginPageVisible();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(testUserName);
});

test('can log in with valid credentials', {tag: '@smoke'}, async ({umbracoUi}) => {
  // Act
  await umbracoUi.login.loginWithCredentials(adminEmail, adminPassword);

  // Assert
  await umbracoUi.login.isBackOfficeMainVisible(true);
});

test('cannot log in with invalid password', async ({umbracoUi}) => {
  // Act
  await umbracoUi.login.loginWithCredentials(adminEmail, wrongPassword);

  // Assert
  await umbracoUi.login.isLoginPageVisible();
  await umbracoUi.login.doesLoginErrorMessageHaveText(ConstantHelper.loginErrorMessages.invalidCredentials);
});

test('cannot log in with unknown email', async ({umbracoUi}) => {
  // Act
  await umbracoUi.login.loginWithCredentials(unknownEmail, adminPassword);

  // Assert
  await umbracoUi.login.isLoginPageVisible();
  await umbracoUi.login.doesLoginErrorMessageHaveText(ConstantHelper.loginErrorMessages.invalidCredentials);
});

test('cannot login with with empty account details', async ({umbracoUi}) => {
  // Act
  await umbracoUi.login.clickLoginButton();

  // Assert
  await umbracoUi.login.isLoginPageVisible();
  await umbracoUi.login.doesUsernameInputErrorHaveText(ConstantHelper.loginErrorMessages.emptyEmail);
  await umbracoUi.login.doesPasswordInputErrorHaveText(ConstantHelper.loginErrorMessages.emptyPassword);
});

test('cannot log in with disabled account', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.loginToAdminUser();
  const userGroup = await umbracoApi.userGroup.getByName(userGroupName);
  const userId = await umbracoApi.user.createDefaultUser(testUserName, testUserEmail, [userGroup.id]);
  await umbracoApi.user.updatePassword(userId, testUserPassword);
  await umbracoApi.user.disable([userId]);
  await umbracoApi.resetAuthState();
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.login.loginWithCredentials(testUserEmail, testUserPassword);

  // Assert
  await umbracoUi.login.isLoginPageVisible();
  await umbracoUi.login.doesLoginErrorMessageHaveText(ConstantHelper.loginErrorMessages.lockedAccount);
});

test('cannot log in with locked-out account', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.loginToAdminUser();
  const userGroup = await umbracoApi.userGroup.getByName(userGroupName);
  const userId = await umbracoApi.user.createDefaultUser(testUserName, testUserEmail, [userGroup.id]);
  await umbracoApi.user.updatePassword(userId, testUserPassword);
  await umbracoApi.user.lockOutByFailedLogins(testUserEmail);
  await umbracoApi.resetAuthState();
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.login.loginWithCredentials(testUserEmail, testUserPassword);

  // Assert
  await umbracoUi.login.isLoginPageVisible();
  await umbracoUi.login.doesLoginErrorMessageHaveText(ConstantHelper.loginErrorMessages.lockedAccount);
});
