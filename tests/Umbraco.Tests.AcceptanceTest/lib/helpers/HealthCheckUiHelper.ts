import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class HealthCheckUiHelper extends UiBaseLocators {
  private readonly healthCheckTab: Locator;
  private readonly healthCheckGroupBox: Locator;
  private readonly performanceAllChecksBtn: Locator;
  private readonly positiveTag: string;
  private readonly warningTag: string;
  private readonly dangerTag: string;
  private readonly healthCheckResultTag: Locator;
  private readonly headline: Locator;
  private readonly healthCheckGroup: Locator;
  private readonly performChecksBtn: Locator;
  private readonly healthCheckBox: Locator;

  constructor(page: Page) {
    super(page);
    this.healthCheckTab = page.getByRole('tab', {name: 'Health Check'});
    this.healthCheckGroupBox = page.locator('umb-health-check-group-box-overview');
    this.performanceAllChecksBtn = page.getByLabel('Perform all checks');
    this.positiveTag = 'uui-tag[color="positive"]';
    this.warningTag = 'uui-tag[color="warning"]';
    this.dangerTag = 'uui-tag[color="danger"]';
    this.healthCheckResultTag = page.locator(`${this.positiveTag}, ${this.warningTag}, ${this.dangerTag}`);
    this.headline = page.locator('#headline');
    this.healthCheckGroup = page.locator('umb-dashboard-health-check-group');
    this.performChecksBtn = page.getByLabel('Perform checks');
    this.healthCheckBox = page.locator('umb-dashboard-health-check-group uui-box');
  }

  async clickHealthCheckTab() {
    await this.click(this.healthCheckTab);
  }

  async checkHealthCheckGroupCount() {
    await this.isVisible(this.healthCheckGroupBox.first());
    return this.healthCheckGroupBox.count();
  }

  async clickPerformanceAllChecksButton() {
    await this.click(this.performanceAllChecksBtn);
  }

  async clickPerformAllChecksButtonAndWaitForResults() {
    await this.click(this.performanceAllChecksBtn);
    await this.waitForVisible(this.healthCheckResultTag.first());
  }

  async clickHealthCheckGroupByName(groupName: string) {
    await this.click(this.page.getByRole('link', {name: groupName}));
  }

  async isHealthCheckGroupVisible(groupName: string) {
    await this.isVisible(this.healthCheckGroupBox.getByText(groupName));
  }

  async doesHealthCheckGroupHaveSuccessItemsCount(healthCheckGroupName: string, count: number) {
    return expect(this.healthCheckGroupBox.filter({has: this.page.getByText(healthCheckGroupName)}).locator(this.positiveTag)).toHaveText(count.toString());
  }

  async doesHealthCheckGroupHaveWarningItemsCount(healthCheckGroupName: string, count: number) {
    return expect(this.healthCheckGroupBox.filter({has: this.page.getByText(healthCheckGroupName)}).locator(this.warningTag)).toHaveText(count.toString());
  }

  async doesHealthCheckGroupHaveErrorItemsCount(healthCheckGroupName: string, count: number) {
    return expect(this.healthCheckGroupBox.filter({has: this.page.getByText(healthCheckGroupName)}).locator(this.dangerTag)).toHaveText(count.toString());
  }

  async isHealthCheckNameVisible(name: string) {
    return await this.isVisible(this.headline.filter({hasText: name}));
  }

  async isHealthCheckDescriptionVisible(description: string) {
    return await this.isVisible(this.healthCheckGroup.getByText(description));
  }

  async clickPerformChecksButton() {
    await this.click(this.performChecksBtn);
  }

  async doesHealthCheckHaveResultMessage(checkName: string, message: string) {
    const resultDescriptionLocator = this.healthCheckBox.filter({has: this.page.locator('#headline', {hasText: checkName})}).locator('.check-result-description');
    await expect(resultDescriptionLocator).toContainText(message);
  }

  async isHealthCheckReadMoreLinkVisible(checkName: string, isVisible = true) {
    const readMoreLocator = this.healthCheckBox.filter({has: this.page.locator('#headline', {hasText: checkName})}).getByLabel('Read more');
    await expect(readMoreLocator).toBeVisible({visible: isVisible});
  }
}
