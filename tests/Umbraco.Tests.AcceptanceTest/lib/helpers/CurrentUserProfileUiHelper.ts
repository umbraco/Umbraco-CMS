import {Locator, Page} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class CurrentUserProfileUiHelper extends UiBaseLocators {
  private readonly changePasswordBtn: Locator;
  private readonly editBtn: Locator;
  private readonly currentUserWorkspace: Locator;
  private readonly workspaceSaveBtn: Locator;
  private readonly workspaceCloseBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.changePasswordBtn = page.getByLabel('Change password');
    this.editBtn = page.locator('umb-current-user-modal').getByLabel('Edit', {exact: true});
    this.currentUserWorkspace = page.locator('umb-current-user-workspace-editor');
    this.workspaceSaveBtn = this.currentUserWorkspace.getByLabel('Save');
    this.workspaceCloseBtn = this.currentUserWorkspace.getByLabel('Close');
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

  async clickEditButton() {
    await this.click(this.editBtn);
  }

  async isCurrentUserWorkspaceVisible(isVisible: boolean = true) {
    await this.isVisible(this.currentUserWorkspace, isVisible);
  }

  async clickSaveWorkspaceButton() {
    await this.click(this.workspaceSaveBtn);
  }

  async clickCloseWorkspaceButton() {
    await this.click(this.workspaceCloseBtn);
  }

  async clickSaveWorkspaceButtonAndWaitForProfileUpdate() {
    return await this.waitForResponseAfterExecutingPromise(
      ConstantHelper.apiEndpoints.currentUserProfile,
      this.clickSaveWorkspaceButton(),
      ConstantHelper.statusCodes.ok
    );
  }

  async clickSaveWorkspaceButtonAndWaitForAvatarDelete() {
    return await this.waitForResponseAfterExecutingPromise(
      ConstantHelper.apiEndpoints.currentUserAvatar,
      this.clickSaveWorkspaceButton(),
      ConstantHelper.statusCodes.ok
    );
  }
}
