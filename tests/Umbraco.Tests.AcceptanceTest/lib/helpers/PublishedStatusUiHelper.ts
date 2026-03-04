import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class PublishedStatusUiHelper extends UiBaseLocators {
  private readonly publishedStatusTab: Locator;
  private readonly refreshStatusBtn: Locator;
  private readonly reloadMemoryCacheBtn: Locator;
  private readonly rebuildDatabaseCacheBtn: Locator;
  private readonly snapshotInternalCacheBtn: Locator;
  private readonly continueBtn: Locator;
  private readonly publishedCacheBox: Locator;

  constructor(page: Page) {
    super(page);
    this.publishedStatusTab = page.getByRole('tab', {name: 'Published Status'});
    this.refreshStatusBtn = page.getByLabel('Refresh Status');
    this.reloadMemoryCacheBtn = page.getByLabel('Reload Memory Cache');
    this.rebuildDatabaseCacheBtn = page.getByLabel('Rebuild Database Cache');
    this.snapshotInternalCacheBtn = page.getByLabel('Snapshot Internal Cache');
    this.continueBtn = page.locator('#confirm').getByLabel('Continue');
    this.publishedCacheBox = page.locator('[headline="Published Cache Status"]')
  }

  async clickPublishedStatusTab() {
    await this.click(this.publishedStatusTab);
  }

  async clickRefreshStatusButton() {
    await this.click(this.refreshStatusBtn);
  }

  async clickReloadMemoryCacheButton() {
    await this.click(this.reloadMemoryCacheBtn);
  }

  async clickRebuildDatabaseCacheButton() {
    await this.click(this.rebuildDatabaseCacheBtn);
  }

  async clickSnapshotInternalCacheButton() {
    await this.click(this.snapshotInternalCacheBtn);
  }

  async clickContinueButton() {
    await this.click(this.continueBtn);
  }

  async isPublishedCacheStatusVisible(status: string) {
    return await this.isVisible(this.publishedCacheBox.getByText(status));
  }
}
