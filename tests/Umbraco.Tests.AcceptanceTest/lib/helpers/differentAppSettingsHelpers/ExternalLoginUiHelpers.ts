import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "../UiBaseLocators";

export class ExternalLoginUiHelpers extends UiBaseLocators {
  private readonly azureADB2CSignInBtn: Locator;
  private readonly azureADB2CEmailTxt: Locator;
  private readonly azureADB2CPasswordTxt: Locator;
  private readonly signInBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.azureADB2CSignInBtn = page.locator('umb-auth-provider-default').getByText('Sign in with Azure AD B2C');
    this.azureADB2CEmailTxt = page.locator('#email');
    this.azureADB2CPasswordTxt = page.locator('#password');
    this.signInBtn = page.getByRole('button', {name: 'Sign in'});
  }

  async clickSignInWithAzureADB2CButton() {
    await this.click(this.azureADB2CSignInBtn);
  }

  async enterAzureADB2CEmail(email: string) {
    await this.enterText(this.azureADB2CEmailTxt, email);
  }

  async enterAzureADB2CPassword(password: string) {
    await this.enterText(this.azureADB2CPasswordTxt, password);
  }

  async clickSignInButton() {
    await this.click(this.signInBtn);
  }
}
