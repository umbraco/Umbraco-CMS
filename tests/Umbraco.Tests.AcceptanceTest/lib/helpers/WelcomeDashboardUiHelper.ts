import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class WelcomeDashboardUiHelper extends UiBaseLocators {
  private readonly welcomeTab: Locator;
  private readonly welcomeBox: Locator;


  constructor(page: Page) {
    super(page);
    this.welcomeTab = page.getByRole('tab', {name: 'Welcome'});
    this.welcomeBox = page.locator('uui-box');
  }

  async clickWelcomeTab() {
    await this.click(this.welcomeTab);
  }

  async doesButtonWithLabelInBoxHaveLink(label: string, boxName: string, link: string) {
    return expect(this.welcomeBox.filter({hasText: boxName}).getByLabel(label)).toHaveAttribute('href', link);
  }
}
