import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class RedirectManagementUiHelper extends UiBaseLocators {
  private readonly redirectManagementTab: Locator;
  private readonly enableURLTrackerBtn: Locator;
  private readonly disableURLTrackerBtn: Locator;
  private readonly originalUrlTxt: Locator;
  private readonly searchBtn: Locator;
  private readonly firstDeleteButton: Locator;
  private readonly redirectManagementRows: Locator;

  constructor(page: Page) {
    super(page);
    this.redirectManagementTab = page.getByRole('tab', {name: 'Redirect URL Management'});
    this.enableURLTrackerBtn = page.getByLabel('Enable URL tracker');
    this.disableURLTrackerBtn = page.getByLabel('Disable URL tracker');
    this.originalUrlTxt = page.getByLabel('Original URL');
    this.searchBtn = page.getByLabel('Search', { exact: true });
    this.firstDeleteButton = page.locator('uui-button[label="Delete"]').first().locator('svg');
    this.redirectManagementRows = page.locator('umb-dashboard-redirect-management uui-table-row');
  }

  async clickRedirectManagementTab() {
    await this.click(this.redirectManagementTab);
  }

  async clickEnableURLTrackerButton() {
    await this.click(this.enableURLTrackerBtn);
  }

  async clickDisableURLTrackerButton() {
    await this.click(this.disableURLTrackerBtn);
  }

  async enterOriginalUrl(url: string) {
    await this.enterText(this.originalUrlTxt, url);
  }

  async clickSearchButton() {
    await this.click(this.searchBtn);
  }

  async deleteFirstRedirectURL() {
    await this.click(this.firstDeleteButton);
    await this.clickConfirmToDeleteButton();
  }

  async doesRedirectManagementRowsHaveCount(itemCount: number) {
    await this.hasCount(this.redirectManagementRows, itemCount);
  }
}
