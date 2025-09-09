import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const azureEmail = process.env.AZUREADB2CTESTUSEREMAIL;
const azurePassword = process.env.AZUREADB2CTESTUSERPASSWORD;

// Really simple test to check if we can log in using Azure AD B2C
test('Log in to Umbraco using Azure AD B2C', async ({umbracoUi}) => {
  test.slow();
  // Arrange
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.externalLoginUiHelpers.clickSignInWithAzureADB2CButton();
  await umbracoUi.externalLoginUiHelpers.enterAzureADB2CEmail(azureEmail);
  await umbracoUi.externalLoginUiHelpers.enterAzureADB2CPassword(azurePassword);
  await umbracoUi.externalLoginUiHelpers.clickSignInButton();

  // Assert
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});
