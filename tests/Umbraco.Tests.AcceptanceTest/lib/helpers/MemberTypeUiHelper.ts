import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";
import {Locator, Page} from "@playwright/test";

export class MemberTypeUiHelper extends UiBaseLocators {
  private readonly memberTypeBtn: Locator;
  private readonly memberTypesMenu: Locator;
  private readonly memberTypeTreeRoot: Locator;

  constructor(page: Page) {
    super(page);
    this.memberTypeBtn = this.createOptionActionListModal.locator('[name="Member Type..."]');
    this.memberTypesMenu = page.locator('#menu-item').getByRole('link', {name: 'Member Types'});
    this.memberTypeTreeRoot = page.locator('[alias="Umb.TreeItem.MemberType"]').locator('uui-menu-item[label="Member Types"]');
  }

  async clickActionsMenuForMemberType(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async clickActionsMenuAtRoot() {
    await this.clickActionsMenuForMemberType("Member Types");
  }

  async clickRootFolderCaretButton() {
    await this.openCaretButtonForName("Member Types");
  }

  async clickMemberTypeButton() {
    await this.click(this.memberTypeBtn);
  }

  async isMemberTypeTreeItemVisible(name: string, isVisible: boolean = true) {
    const hasShowChildren = await this.memberTypeTreeRoot.getAttribute('show-children') !== null;

    if (!hasShowChildren) {
      await this.click(this.memberTypeTreeRoot.locator(this.caretBtn).first());
    }

    await this.isTreeItemVisible(name, isVisible);
  }

  async reloadMemberTypeTree() {
    await this.reloadTree('Member Types');
  }

  async goToMemberType(memberTypeName: string) {
    await this.clickRootFolderCaretButton();
    await this.clickLabelWithName(memberTypeName);
  }

  async enterMemberTypeName(name: string) {
    await this.enterText(this.enterAName, name);
  }

  async clickSaveButtonAndWaitForMemberTypeToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.memberType, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForMemberTypeToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.memberType, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForMemberTypeToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.memberType, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async clickDeleteAndConfirmButtonAndWaitForMemberTypeToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.memberType, this.clickDeleteAndConfirmButton(), ConstantHelper.statusCodes.ok);
  }

  async clickMemberTypesMenu() {
    await this.click(this.memberTypesMenu);
  }

  async clickConfirmCreateFolderButtonAndWaitForMemberTypeToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.memberTypeFolder, this.clickConfirmCreateFolderButton(), ConstantHelper.statusCodes.created);
  }

  async clickConfirmRenameButtonAndWaitForMemberTypeToBeRenamed() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.memberTypeFolder, this.clickConfirmRenameButton(), ConstantHelper.statusCodes.ok);
  }
}
