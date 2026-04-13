import {Locator, Page, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {umbracoConfig} from "../umbraco.config";

// Page object for the public-facing member authentication forms rendered by the embedded
// Razor snippets (Login, RegisterMember, EditProfile, LoginStatus). Field selectors target
// the form input names produced by the snippets, which use the [Bind(Prefix = "...")]
// convention on each surface controller action.
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

  // Markers injected by TemplateApiHelper.createMember*Template; data-mark is the project-wide
  // test-id attribute (see playwright.config.ts testIdAttribute).
  private readonly authStateMarker: Locator;
  private readonly authNameMarker: Locator;

  // Default ASP.NET Identity ApplicationScheme cookie used by the Umbraco member auth pipeline
  // (see ConfigureMemberCookieOptions).
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

  async navigateToPage(contentURL: string) {
    await this.page.goto(umbracoConfig.environment.baseUrl + contentURL);
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

  async expectRegisterSuccess() {
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

  async expectProfileSuccess() {
    await expect(this.profileSuccessIndicator).toBeVisible();
  }

  // The profile snippet wraps the form in @if (profileModel != null), so the button's absence
  // is the observable signal that BuildForCurrentMemberAsync returned null (anonymous user).
  async expectProfileFormHidden() {
    await expect(this.profileBtn).toHaveCount(0);
  }

  async clickLogoutButton() {
    await this.click(this.logoutBtn);
    await this.waitForLoadState();
  }

  // Clears only the member auth cookie, leaving back-office cookies (which umbracoApi depends
  // on) intact.
  async clearMemberAuthCookie() {
    await this.page.context().clearCookies({name: this.memberAuthCookieName});
  }

  // The marker prints memberIdentity.Name, which ASP.NET Identity populates from the member's
  // UserName (not the member's display / variant name).
  async expectAuthenticated(expectedUsername: string) {
    await expect(this.authStateMarker).toHaveText('authenticated');
    await expect(this.authNameMarker).toHaveText(expectedUsername);
  }

  async expectAnonymous() {
    await expect(this.authStateMarker).toHaveText('anonymous');
  }

  // <div asp-validation-summary="All"> renders as <div class="text-danger"> with a <ul><li>
  // for each error.
  async expectValidationError(message: string) {
    await expect(this.page.locator('div.text-danger', {hasText: message}).first()).toBeVisible();
  }
}
