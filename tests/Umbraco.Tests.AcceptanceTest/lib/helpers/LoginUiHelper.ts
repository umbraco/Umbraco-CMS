import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";
import {umbracoConfig} from "../umbraco.config";

export class LoginUiHelper extends UiBaseLocators {
  private readonly emailTxt: Locator;
  private readonly passwordTxt: Locator;
  private readonly loginBtn: Locator;
  private readonly loginErrorMessage: Locator;
  private readonly logoutBtn: Locator;
  private readonly forgottenPasswordBtn: Locator;
  private readonly resetEmailTxt: Locator;
  private readonly resetSubmitBtn: Locator;
  private readonly resetConfirmation: Locator;
  private readonly resetNewPasswordTxt: Locator;
  private readonly resetConfirmNewPasswordTxt: Locator;
  private readonly newPasswordContinueBtn: Locator;
  private readonly newPasswordSuccess: Locator;
  private readonly newPasswordErrorLayout: Locator;

  constructor(page: Page) {
    super(page);
    this.emailTxt = page.locator('[name="username"]');
    this.passwordTxt = page.locator('[name="password"]');
    this.loginBtn = page.getByLabel('Login');
    this.loginErrorMessage = page.locator('umb-login-page .text-error');
    this.logoutBtn = page.locator('umb-current-user-modal uui-button').getByText('Logout');
    this.forgottenPasswordBtn = page.locator('umb-login-page #forgot-password');
    this.resetEmailTxt = page.locator('umb-reset-password-page input[name="email"]');
    this.resetSubmitBtn = page.locator('umb-reset-password-page uui-button[type="submit"]');
    this.resetConfirmation = page.locator('umb-reset-password-page umb-confirmation-layout');
    this.resetNewPasswordTxt = page.locator('umb-new-password-layout input[name="password"]');
    this.resetConfirmNewPasswordTxt = page.locator('umb-new-password-layout input[name="confirmPassword"]');
    this.newPasswordContinueBtn = page.locator('umb-new-password-layout uui-button[type="submit"]');
    this.newPasswordSuccess = page.locator('umb-new-password-page umb-confirmation-layout');
    this.newPasswordErrorLayout = page.locator('umb-new-password-page umb-error-layout');
  }

  async enterEmail(email: string) {
    await this.enterText(this.emailTxt, email, {verify: true, timeout: ConstantHelper.timeout.navigation});
  }

  async enterPassword(password: string) {
    await this.enterText(this.passwordTxt, password);
  }

  async clickLoginButton() {
    await this.click(this.loginBtn);
  }

  async loginWithCredentials(email: string, password: string) {
    await this.enterEmail(email);
    await this.enterPassword(password);
    await this.clickLoginButton();
  }

  async isOnLoginPage(isVisible: boolean = true) {
    await this.isVisible(this.emailTxt, isVisible);
    await this.isVisible(this.passwordTxt, isVisible);
  }

  async doesLoginErrorMessageHaveText(message: string) {
    await this.containsText(this.loginErrorMessage, message);
  }

  async clickLogoutButtonAndWaitForUserLogout() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.revoke, this.logoutBtn.click(), ConstantHelper.statusCodes.ok);
  }

  async clickForgottenPasswordButton() {
    await this.click(this.forgottenPasswordBtn);
  }

  async enterResetEmail(email: string) {
    await this.enterText(this.resetEmailTxt, email);
  }

  async clickResetSubmitButton() {
    await this.click(this.resetSubmitBtn);
  }

  async submitForgotPasswordForEmail(email: string) {
    await this.clickForgottenPasswordButton();
    await this.enterResetEmail(email);
    await this.clickResetSubmitButton();
  }

  async isResetConfirmationVisible(isVisible: boolean = true) {
    await this.isVisible(this.resetConfirmation, isVisible);
  }

  async doesResetConfirmationHaveText(message: string) {
    await this.containsText(this.resetConfirmation, message);
  }

  async enterNewPassword(password: string) {
    await this.enterText(this.resetNewPasswordTxt, password);
  }

  async enterConfirmNewPassword(password: string) {
    await this.enterText(this.resetConfirmNewPasswordTxt, password);
  }

  async clickNewPasswordContinueButton() {
    await this.click(this.newPasswordContinueBtn);
  }

  async submitNewPassword(password: string) {
    await this.enterNewPassword(password);
    await this.enterConfirmNewPassword(password);
    await this.clickNewPasswordContinueButton();
  }

  async isNewPasswordSuccessVisible(isVisible: boolean = true) {
    await this.isVisible(this.newPasswordSuccess, isVisible);
  }

  async doesNewPasswordErrorHaveText(message: string) {
    await this.containsText(this.newPasswordErrorLayout, message);
  }

  async goToResetPasswordPage(userId: string, resetCode: string) {
    const baseUrl = umbracoConfig.environment.baseUrl;
    await this.page.goto(`${baseUrl}/umbraco/login?flow=reset-password&userId=${userId}&resetCode=${resetCode}`);
  }

  async goToResetPasswordFromEmailUrl(emailUrl: string) {
    const search = new URL(emailUrl).search;
    await this.page.goto(umbracoConfig.environment.baseUrl + '/umbraco/login' + search);
  }
}
