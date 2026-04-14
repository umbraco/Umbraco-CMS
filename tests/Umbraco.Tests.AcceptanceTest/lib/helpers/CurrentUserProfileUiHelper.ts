import {expect, Locator, Page} from "@playwright/test"
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class CurrentUserProfileUiHelper extends UiBaseLocators {
  private readonly changePasswordBtn: Locator;
  private readonly historyEntries: Locator;

  constructor(page: Page) {
    super(page);
    this.changePasswordBtn = page.getByLabel('Change password');
    this.historyEntries = page.locator('umb-current-user-history-user-profile-app uui-ref-node');
  }

  async getHistoryEntryByName(name: string): Promise<Locator> {
    return this.historyEntries.filter({has: this.page.locator(`[name="${name}"]`)});
  }

  async getHistoryEntryDetail(name: string): Promise<string | null> {
    const entry = await this.getHistoryEntryByName(name);
    return entry.getAttribute('detail');
  }

  async isHistoryEntryVisible(name: string, isVisible: boolean = true): Promise<void> {
    const entry = await this.getHistoryEntryByName(name);
    if (isVisible) {
      await expect(entry).toBeVisible();
    } else {
      await expect(entry).not.toBeVisible();
    }
  }

  async countHistoryEntriesWithName(name: string): Promise<number> {
    const entry = await this.getHistoryEntryByName(name);
    return entry.count();
  }

  async clickChangePasswordButton() {
    await this.click(this.changePasswordBtn);
  }

  async changePassword(currentPassword: string, newPassword: string) {
    await this.enterText(this.currentPasswordTxt, currentPassword);
    await this.enterText(this.newPasswordTxt, newPassword);
    await this.enterText(this.confirmPasswordTxt, newPassword);
    await this.clickConfirmButton();
  }

  async changePasswordAndWaitForSuccess(currentPassword: string, newPassword: string) {
    await this.waitForVisible(this.currentPasswordTxt);
    await this.enterText(this.currentPasswordTxt, currentPassword);
    await this.enterText(this.newPasswordTxt, newPassword);
    await this.enterText(this.confirmPasswordTxt, newPassword);
    return await this.waitForResponseAfterExecutingPromise(
      ConstantHelper.apiEndpoints.currentUser + '/change-password',
      this.clickConfirmButton(),
      ConstantHelper.statusCodes.ok
    );
  }
}