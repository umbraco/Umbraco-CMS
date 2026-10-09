import {Locator, Page, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class MemberAuthenticationUiHelper extends UiBaseLocators {
  private readonly loginUsernameTxt: Locator;
  private readonly loginPasswordTxt: Locator;
  private readonly loginRememberMeChk: Locator;
  private readonly loginBtn: Locator;
  private readonly registerNameTxt: Locator;
  private readonly registerEmailTxt: Locator;
  private readonly registerPasswordTxt: Locator;
  private readonly registerConfirmPasswordTxt: Locator;
  private readonly registerBtn: Locator;
  private readonly registerSuccessIndicator: Locator;
  private readonly profileNameTxt: Locator;
  private readonly profileEmailTxt: Locator;
  private readonly profileBtn: Locator;
  private readonly profileSuccessIndicator: Locator;
  private readonly logoutBtn: Locator;
  private readonly authStateMarker: Locator;
  private readonly authNameMarker: Locator;
  private readonly memberAuthCookieName = '.AspNetCore.Identity.Application';

  constructor(page: Page) {
    super(page);
    this.loginUsernameTxt = page.locator('input[name="loginModel.Username"]');
    this.loginPasswordTxt = page.locator('input[name="loginModel.Password"]');
    this.loginRememberMeChk = page.locator('input[name="loginModel.RememberMe"][type="checkbox"]');
    this.loginBtn = page.getByRole('button', {name: 'Log in'});
    this.registerNameTxt = page.locator('input[name="registerModel.Name"]');
    this.registerEmailTxt = page.locator('input[name="registerModel.Email"]');
    this.registerPasswordTxt = page.locator('input[name="registerModel.Password"]');
    this.registerConfirmPasswordTxt = page.locator('input[name="registerModel.ConfirmPassword"]');
    this.registerBtn = page.getByRole('button', {name: 'Register'});
    this.registerSuccessIndicator = page.locator('p.text-success', {hasText: 'Registration succeeded'});
    this.profileNameTxt = page.locator('input[name="profileModel.Name"]');
    this.profileEmailTxt = page.locator('input[name="profileModel.Email"]');
    this.profileBtn = page.getByRole('button', {name: 'Update'});
    this.profileSuccessIndicator = page.locator('p.text-success', {hasText: 'Profile updated'});
    this.logoutBtn = page.getByRole('button', {name: 'Log out'});
    this.authStateMarker = page.getByTestId('member-auth-state');
    this.authNameMarker = page.getByTestId('member-auth-name');
  }

  async fillLoginForm(username: string, password: string, rememberMe: boolean = false) {
    await this.enterText(this.loginUsernameTxt, username);
    await this.enterText(this.loginPasswordTxt, password);
    if (rememberMe) {
      await this.check(this.loginRememberMeChk);
    }
  }

  async submitLoginForm() {
    await this.click(this.loginBtn);
    await this.waitForLoadState();
  }

  async fillRegisterForm(name: string, email: string, password: string, confirmPassword?: string) {
    await this.enterText(this.registerNameTxt, name);
    await this.enterText(this.registerEmailTxt, email);
    await this.enterText(this.registerPasswordTxt, password);
    await this.enterText(this.registerConfirmPasswordTxt, confirmPassword ?? password);
  }

  async submitRegisterForm() {
    await this.click(this.registerBtn);
    await this.waitForLoadState();
  }

  async doesRegisterSuccessShow() {
    await expect(this.registerSuccessIndicator).toBeVisible();
  }

  async fillProfileForm(name: string, email: string) {
    await this.enterText(this.profileNameTxt, name);
    await this.enterText(this.profileEmailTxt, email);
  }

  async submitProfileForm() {
    await this.click(this.profileBtn);
    await this.waitForLoadState();
  }

  async doesProfileSuccessShow() {
    await expect(this.profileSuccessIndicator).toBeVisible();
  }

  async isProfileFormHidden() {
    await expect(this.profileBtn).toHaveCount(0);
  }

  async clickLogoutButton() {
    await this.click(this.logoutBtn);
    await this.waitForLoadState();
  }

  async clearMemberAuthCookie() {
    await this.page.context().clearCookies({name: this.memberAuthCookieName});
  }

  async isAuthenticated(expectedUsername: string) {
    await expect(this.authStateMarker).toHaveText('authenticated');
    await expect(this.authNameMarker).toHaveText(expectedUsername);
  }

  async isAnonymous() {
    await expect(this.authStateMarker).toHaveText('anonymous');
  }

  async doesValidationErrorShow(message: string) {
    await expect(this.page.locator('div.text-danger', {hasText: message}).first()).toBeVisible();
  }
}
