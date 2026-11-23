import {ConstantHelper, test, UiHelpers} from '@umbraco/acceptance-test-helpers';

const azureEmail = process.env.AZUREADB2CTESTUSEREMAIL;
const azurePassword = process.env.AZUREADB2CTESTUSERPASSWORD;

// Really simple test to check if we can log in using Azure AD B2C
test('Log in to Umbraco using Azure AD B2C', async ({umbracoUi}) => {
  test.slow();
  // Arrange
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.externalLogin.clickSignInWithAzureADB2CButton();
  await umbracoUi.externalLogin.enterAzureADB2CEmail(azureEmail);
  await umbracoUi.externalLogin.enterAzureADB2CPassword(azurePassword);
  await umbracoUi.externalLogin.clickSignInButton();

  // Assert
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});

// Hosting control panels and the Umbraco Cloud portal link to the backoffice in a new tab
test('Log in to Umbraco using Azure AD B2C in a tab that another page opened', async ({umbracoUi}) => {
  test.slow();
  // Arrange
  const backOfficeTab = await umbracoUi.goToBackOfficeInNewTab();
  const backOfficeUi = new UiHelpers(backOfficeTab);

  // Act
  await backOfficeUi.externalLogin.clickSignInWithAzureADB2CButton();
  await backOfficeUi.externalLogin.enterAzureADB2CEmail(azureEmail);
  await backOfficeUi.externalLogin.enterAzureADB2CPassword(azurePassword);
  await backOfficeUi.externalLogin.clickSignInButton();

  // Assert
  await backOfficeUi.content.goToSection(ConstantHelper.sections.content);
});
