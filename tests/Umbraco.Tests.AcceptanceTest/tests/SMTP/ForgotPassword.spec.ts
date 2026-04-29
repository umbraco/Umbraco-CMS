import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const initialPassword = 'InitialPassword123!';
const newPassword = 'NewSecurePassword123!';
const userGroupName = 'Writers';
const resetEmailSubject = 'Umbraco: Reset Password';

let testUserName: string;
let testUserEmail: string;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  const random = crypto.randomUUID();
  testUserName = `Test Forgot Password ${random}`;
  testUserEmail =  `testforgot${random}@acceptance.test`;
  const userGroup = await umbracoApi.userGroup.getByName(userGroupName);
  const userId = await umbracoApi.user.createDefaultUser(testUserName, testUserEmail, [userGroup.id]);
  await umbracoApi.user.updatePassword(userId, initialPassword);
  await umbracoApi.smtp.deleteAllEmails();
  await umbracoApi.resetAuthState();
  await umbracoUi.goToBackOffice();
  await umbracoUi.login.isOnLoginPage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(testUserName);
  await umbracoApi.smtp.deleteAllEmails();
});

test('user can request a password reset email', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.login.submitForgotPasswordForEmail(testUserEmail);

  // Assert
  await umbracoUi.login.doesResetConfirmationHaveText(ConstantHelper.forgottenPasswordMessages.confirmation);
  const email = await umbracoApi.smtp.findEmailToRecipient(testUserEmail);
  expect(email).not.toBeNull();
  expect(email.subject).toBe(resetEmailSubject);
});

test('user can reset password using link from email and log in with new password', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.login.submitForgotPasswordForEmail(testUserEmail);
  await umbracoUi.login.doesResetConfirmationHaveText(ConstantHelper.forgottenPasswordMessages.confirmation);

  // Act
  const emailUrl = await umbracoApi.smtp.extractPasswordResetUrlForRecipient(testUserEmail);
  expect(emailUrl).not.toBeNull();
  await umbracoUi.login.goToResetPasswordFromEmailUrl(emailUrl!);
  await umbracoUi.login.submitNewPassword(newPassword);

  // Assert 
  await umbracoUi.login.isNewPasswordSuccessVisible();
  await umbracoApi.resetAuthState();
  await umbracoUi.goToBackOffice();
  await umbracoUi.login.isOnLoginPage();
  await umbracoUi.login.loginWithCredentials(testUserEmail, newPassword);
  await umbracoUi.login.isBackOfficeMainVisible(true);
});

test('requesting reset for unknown email shows confirmation but sends no email', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const nonExistentEmail = 'does-not-exist@acceptance.test';

  // Act
  await umbracoUi.login.submitForgotPasswordForEmail(nonExistentEmail);

  // Assert 
  await umbracoUi.login.doesResetConfirmationHaveText(ConstantHelper.forgottenPasswordMessages.confirmation);
  const email = await umbracoApi.smtp.findEmailToRecipient(nonExistentEmail);
  expect(email).toBeNull();
});

test('invalid reset link shows the error', async ({umbracoUi}) => {
  // Act
  await umbracoUi.login.goToResetPasswordPage('00000000-0000-0000-0000-000000000001', 'not-a-real-token');

  // Assert
  await umbracoUi.login.doesNewPasswordErrorHaveText(ConstantHelper.forgottenPasswordMessages.notFoundUserError);
});
