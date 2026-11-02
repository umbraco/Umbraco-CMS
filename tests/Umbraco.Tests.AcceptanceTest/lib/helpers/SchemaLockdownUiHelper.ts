import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class SchemaLockdownUiHelper extends UiBaseLocators {
  private readonly lockedWorkspaceActionBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.lockedWorkspaceActionBtn = page.locator('umb-schema-lockdown-workspace-action uui-button[label="Locked"]');
  }

  async isLockedWorkspaceActionVisible(isVisible: boolean = true) {
    await this.isVisible(this.lockedWorkspaceActionBtn, isVisible);
  }

  async clickLockedWorkspaceActionButton() {
    await this.click(this.lockedWorkspaceActionBtn);
  }
}
