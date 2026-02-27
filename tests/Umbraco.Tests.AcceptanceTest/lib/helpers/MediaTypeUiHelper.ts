import {UiBaseLocators} from "./UiBaseLocators";
import {Locator, Page} from "@playwright/test";
import {ConstantHelper} from "./ConstantHelper";

export class MediaTypeUiHelper extends UiBaseLocators {
  private readonly newMediaTypeThreeDotsBtn: Locator;
  private readonly mediaEditPropertyWorkspace: Locator;
  private readonly mediaTypeBtn: Locator;
  private readonly mediaTypesMenu: Locator;
  private readonly mediaTypeTreeRoot: Locator;

  constructor(page: Page) {
    super(page);
    this.newMediaTypeThreeDotsBtn = page.getByLabel('New Media Type…');
    this.mediaEditPropertyWorkspace = page.locator('umb-media-type-workspace-view-edit-property');
    this.mediaTypeBtn = this.createOptionActionListModal.locator('[name="Media Type"]');
    this.mediaTypesMenu = page.locator('#menu-item').getByRole('link', {name: 'Media Types'});
    this.mediaTypeTreeRoot = page.locator('[alias="Umb.TreeItem.MediaType"]').locator('uui-menu-item[label="Media Types"]')
  }

  async clickActionsMenuForMediaType(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async clickActionsMenuAtRoot() {
    await this.clickActionsMenuForMediaType("Media Types");
  }

  async clickRootFolderCaretButton() {
    await this.openCaretButtonForName("Media Types");
  }

  async clickNewMediaTypeButton() {
    await this.click(this.newMediaTypeThreeDotsBtn);
  }

  async isMediaTypeTreeItemVisible(name: string, isVisible: boolean = true) {
    const hasShowChildren = await this.mediaTypeTreeRoot.getAttribute('show-children') !== null;

    if (!hasShowChildren) {
      await this.click(this.mediaTypeTreeRoot.locator(this.caretBtn).first());
    }

    await this.isTreeItemVisible(name, isVisible);
  }

  async reloadMediaTypeTree() {
    await this.reloadTree('Media Types');
  }

  async goToMediaType(mediaTypeName: string) {
    await this.clickRootFolderCaretButton();
    await this.clickLabelWithName(mediaTypeName);
  }

  async enterMediaTypeName(name: string) {
    await this.enterText(this.enterAName, name);
  }

  async enterDescriptionForPropertyEditorWithName(propertyEditorName: string, description: string) {
    await this.mediaEditPropertyWorkspace.filter({hasText: propertyEditorName}).getByLabel('description').fill(description);
  }

  async clickMediaTypeButton() {
    await this.click(this.mediaTypeBtn);
  }

  async clickMediaTypesMenu() {
    await this.click(this.mediaTypesMenu);
  }

  async clickSaveButtonAndWaitForMediaTypeToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.mediaType, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForMediaTypeToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.mediaType, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForMediaTypeToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.mediaType, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async clickDeleteAndConfirmButtonAndWaitForMediaTypeToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.mediaType, this.clickDeleteAndConfirmButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmCreateFolderButtonAndWaitForMediaTypeToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.mediaTypeFolder, this.clickConfirmCreateFolderButton(), ConstantHelper.statusCodes.created);
  }

  async clickConfirmRenameButtonAndWaitForMediaTypeToBeRenamed() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.mediaTypeFolder, this.clickConfirmRenameButton(), ConstantHelper.statusCodes.ok);
  }
}
