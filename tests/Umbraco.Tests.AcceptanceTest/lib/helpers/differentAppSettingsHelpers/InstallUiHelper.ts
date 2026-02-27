import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "../UiBaseLocators";

export class InstallUiHelper extends UiBaseLocators {
  private readonly nameTxt: Locator;
  private readonly emailTxt: Locator;
  private readonly passwordTxt: Locator;
  private readonly databaseTypeInput: Locator;
  private readonly databaseType: Locator;
  private readonly installBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.nameTxt = page.getByLabel('name');
    this.emailTxt = page.getByLabel('email');
    this.passwordTxt = page.getByLabel('password', {exact: true});
    this.databaseTypeInput = page.locator('#database-type').locator('#native');
    this.databaseType = page.locator('#database-type').locator('option:checked');
    this.installBtn = page.getByLabel('Install');
  }

  async goToInstallPage() {
    await this.page.goto(process.env.URL + '/umbraco/install');
  }

  async enterName(name: string) {
    await this.enterText(this.nameTxt, name);
  }

  async enterEmail(email: string) {
    await this.enterText(this.emailTxt, email);
  }

  async enterPassword(password: string) {
    await this.enterText(this.passwordTxt, password);
  }

  async setDatabaseType(databaseType: string) {
    await this.databaseTypeInput.selectOption(databaseType)
  }

  async doesDatabaseHaveType(databaseType: string) {
    await this.hasText(this.databaseType, databaseType);
  }

  async clickInstallButton() {
    await this.click(this.installBtn);
  }
}
