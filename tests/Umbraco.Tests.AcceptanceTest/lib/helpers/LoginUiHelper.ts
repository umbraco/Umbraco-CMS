import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class LoginUiHelper extends UiBaseLocators {
  private readonly emailTxt: Locator;
  private readonly passwordTxt: Locator;
  private readonly loginBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.emailTxt = page.locator('[name="username"]');
    this.passwordTxt = page.locator('[name="password"]');
    this.loginBtn = page.getByLabel('Login');
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
}
