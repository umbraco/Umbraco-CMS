import {test} from '@umbraco/playwright-testhelpers';

test('can register new user with google external provider', async () => {
  // Arrange
  // Preconditions: User has an existing account linked with Google.
  // 1. Go to the login page.

  // Act
  // 2. Click to "Sign in with Google" button
  // 3. Enter valid Google credentials

  // Assert
  // 4. User is redirected to the Umbraco dashboard
  // 5. User account is created and linked successfully.
});

test('can login with google external provider', async () => {
  // Arrange
  // Preconditions: User has no existing Umbraco account.
  // 1. Go to the login page.

  // Act
  // 2. Click to "Sign in with Google" button
  // 3. Enter valid Google credentials

  // Assert
  // 4. User is redirected to the Umbraco dashboard
  // 5. User account is linked successfully.
});

test('cannot login with invalid google credentials', async () => {
  // Arrange
  // 1. Go to the login page.

  // Act
  // 2. Click to "Sign in with Google" button
  // 3. Enter invalid credentials

  // Assert
  // 4. An error message is displayed
  // 5. The user remains on the login page.
});

test('can logout from external provider', async () => {
  // Arrange
  // 1. Log in backoffice using an Google external provider.

  // Act
  // 2. Click to "Logout" button

  // Assert
  // 3. User is logged out from Umbraco and the external provider
});

test('can unlink an external account from the Umbraco account.', async () => {
  // Arrange
  // 1. Log in backoffice using an Google external provider.

  // Act
  // 2. Click "Unlink" button
  // 3. Confirm the unlinking action

  // Assert
  // 4. The external account is successfully unlinked, and a confirmation message is displayed.
});

test('cannot log in with an external provider if the account is not linked', async () => {
  // Arrange
  // Preconditions: User has not linked their Umbraco account with any external provider.
  // 1. Go to the Login page

  // Act
  // 2. Click to "Sign in with Google" button
  // 3. Enter valid Google credentials

  // Assert
  // 4. An error message should indicate that the external account is not linked to any Umbraco account.
});

