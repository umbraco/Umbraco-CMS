import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class PreviewUiHelper extends UiBaseLocators {
  private previewPage: Page | null = null;
  private exitBtn: Locator;
  private previewWebsiteBtn: Locator;
  private previewIframe: Locator;
  private deviceBtn: Locator;
  private devicePopover: Locator;
  private cultureBtn: Locator;
  private culturePopover: Locator;

  constructor(page: Page) {
    super(page);
    this.exitBtn = page.locator('umb-preview-exit').locator('uui-button');
    this.previewWebsiteBtn = page.locator('umb-preview-open-website').locator('uui-button');
    this.previewIframe = page.locator('umb-preview iframe');
    this.deviceBtn = page.locator('umb-preview-device').locator('uui-button');
    this.devicePopover = page.locator('umb-preview-device').locator('uui-popover-container');
    this.cultureBtn = page.locator('umb-preview-culture').locator('uui-button');
    this.culturePopover = page.locator('umb-preview-culture').locator('uui-popover-container');
  }

  async waitForPreviewPage() {
    this.previewPage = await this.page.waitForEvent('popup');
    await this.previewPage.waitForLoadState('load');
    this.exitBtn = this.previewPage.locator('umb-preview-exit').locator('uui-button');
    this.previewWebsiteBtn = this.previewPage.locator('umb-preview-open-website').locator('uui-button');
    this.previewIframe = this.previewPage.locator('umb-preview iframe');
    this.deviceBtn = this.previewPage.locator('umb-preview-device').locator('uui-button');
    this.devicePopover = this.previewPage.locator('umb-preview-device').locator('uui-popover-container');
    this.cultureBtn = this.previewPage.locator('umb-preview-culture').locator('uui-button');
    this.culturePopover = this.previewPage.locator('umb-preview-culture').locator('uui-popover-container');
  }

  async isExitButtonVisible(isVisible: boolean = true) {
    await this.isVisible(this.exitBtn, isVisible, ConstantHelper.timeout.long);
  }

  async isPreviewWebsiteButtonVisible(isVisible: boolean = true) {
    await this.isVisible(this.previewWebsiteBtn, isVisible, ConstantHelper.timeout.long);
  }

  async isIframeAttached() {
    await expect(this.previewIframe).toBeAttached({timeout: ConstantHelper.timeout.long});
  }

  async clickExitButton() {
    await this.click(this.exitBtn);
  }

  async clickPreviewWebsiteButtonAndWaitForWebsite(): Promise<Page> {
    const targetPage = this.previewPage ?? this.page;
    const websitePromise = targetPage.waitForEvent('popup');
    await this.click(this.previewWebsiteBtn);
    const websitePage = await websitePromise;
    await websitePage.waitForLoadState('load');
    return websitePage;
  }

  async isPreviewPageClosed() {
    await expect(async () => {
      expect(this.previewPage?.isClosed()).toBeTruthy();
    }).toPass({timeout: ConstantHelper.timeout.long});
  }

  async clickDeviceButton() {
    await this.click(this.deviceBtn);
  }

  async isDeviceButtonVisible(isVisible: boolean = true) {
    await this.isVisible(this.deviceBtn, isVisible, ConstantHelper.timeout.long);
  }

  async clickDeviceByName(deviceName: string) {
    const menuItem = this.devicePopover.locator('uui-menu-item', {hasText: deviceName});
    await this.click(menuItem);
  }

  async isDeviceActive(deviceName: string) {
    const menuItem = this.devicePopover.locator('uui-menu-item', {hasText: deviceName});
    await expect(menuItem).toHaveAttribute('active', '', {timeout: ConstantHelper.timeout.long});
  }

  async doesIframeContainText(text: string) {
    const targetPage = this.previewPage ?? this.page;
    const frame = targetPage.frameLocator('umb-preview iframe');
    await expect(frame.locator('body')).toContainText(text, {timeout: ConstantHelper.timeout.long});
  }

  async clickCultureButton() {
    await this.click(this.cultureBtn);
  }

  async clickCultureByName(cultureName: string) {
    const menuItem = this.culturePopover.locator('uui-menu-item', {hasText: cultureName});
    await this.click(menuItem);
  }

  async isCultureActive(cultureName: string) {
    const menuItem = this.culturePopover.locator('uui-menu-item', {hasText: cultureName});
    await expect(menuItem).toHaveAttribute('active', '', {timeout: ConstantHelper.timeout.long});
  }
}
