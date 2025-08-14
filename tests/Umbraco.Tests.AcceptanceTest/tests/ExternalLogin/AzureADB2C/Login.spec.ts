// To be able to test different databases, we need to set an additional UnattendedInstallConfig up because we would have to start from scratch, otherwise we would be using the same database.
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const azureEmail = process.env.AZUREB2C__TESTUSEREMAIL;
const azurePassword = process.env.AZUREB2C__TESTUSERPASSWORD;

// Really simple test to check if we can log in using Azure AD B2C
test('Log in to Umbraco using Azure AD B2C', async ({page, umbracoUi}) => {
  test.slow();
  // Arrange
  await umbracoUi.goToBackOffice();

  await page.locator('umb-auth-provider-default').getByText('Sign in with Sign in with Azure AD B2C').click();


  await page.locator('#email').fill(azureEmail);
  await page.locator('#password').fill(azurePassword);
  await page.getByRole('button', { name: 'Sign in' }).click();

  // Assert
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});
