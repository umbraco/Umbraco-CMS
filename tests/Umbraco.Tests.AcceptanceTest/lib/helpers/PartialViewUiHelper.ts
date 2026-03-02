import {Page, Locator} from "@playwright/test"
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class PartialViewUiHelper extends UiBaseLocators {
  private readonly newEmptyPartialViewBtn: Locator;
  private readonly newPartialViewFromSnippetBtn: Locator;
  private readonly partialViewTree: Locator;
  private readonly partialViewUiLoader: Locator;
  private readonly newFolderThreeDots: Locator;
  private readonly partialViewCreateModal: Locator;

  constructor(page: Page) {
    super(page);
    this.partialViewCreateModal = page.locator('umb-partial-view-create-options-modal');
    this.newEmptyPartialViewBtn = this.partialViewCreateModal.locator('uui-menu-item', {hasText: 'Empty partial view'});
    this.newPartialViewFromSnippetBtn = this.partialViewCreateModal.locator('uui-menu-item', {hasText: 'Partial view from snippet'});
    this.partialViewTree = page.locator('umb-tree[alias="Umb.Tree.PartialView"]');
    this.partialViewUiLoader = page.locator('uui-loader');
    this.newFolderThreeDots = this.partialViewCreateModal.locator('uui-menu-item', {hasText: 'Folder'});
  }

  async clickActionsMenuForPartialView(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async clickActionsMenuAtRoot() {
    await this.clickActionsMenuForPartialView('Partial Views');
  }

  async clickRootFolderCaretButton() {
    await this.openCaretButtonForName('Partial Views');
  }

  async clickSaveButtonAndWaitForPartialViewToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.partialView, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForPartialViewToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.partialView, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickNewEmptyPartialViewButton() {
    await this.click(this.newEmptyPartialViewBtn);
  }

  async clickNewPartialViewFromSnippetButton() {
    await this.click(this.newPartialViewFromSnippetBtn);
  }

  async enterPartialViewName(partialViewName: string) {
    await this.enterText(this.enterAName, partialViewName);
    await this.hasValue(this.enterAName, partialViewName);
  }

  async enterPartialViewContent(partialViewContent: string) {
    // The waits in this method is currently needed as the test will fail with expects
    await this.waitUntilPartialViewLoaderIsNoLongerVisible();
    await this.enterMonacoEditorValue(partialViewContent);
    // We need this wait, to be sure that the partial view content is loaded.
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async openPartialViewAtRoot(partialViewName: string) {
    await this.reloadPartialViewTree();
    await this.click(this.page.locator(`uui-menu-item[label="${partialViewName}"]`));
    await this.waitForVisible(this.enterAName);
  }

  async createPartialViewFolder(folderName: string) {
    await this.clickCreateOptionsActionMenuOption();
    await this.click(this.newFolderThreeDots);
    await this.enterFolderName(folderName);
    await this.clickConfirmCreateFolderButton();
  }

  async reloadPartialViewTree() {
    await this.reloadTree('Partial Views');
  }

  async waitUntilPartialViewLoaderIsNoLongerVisible() {
    await this.isVisible(this.partialViewUiLoader, false);
  }

  async isPartialViewRootTreeItemVisible(partialView: string, isVisible: boolean = true, toReload: boolean = true) {
    if (toReload) {
      await this.reloadPartialViewTree();
    }
    return await this.isVisible(this.partialViewTree.getByText(partialView, {exact: true}), isVisible);
  }

  async clickConfirmToDeleteButtonAndWaitForPartialViewToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.partialView, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async clickDeleteAndConfirmButtonAndWaitForPartialViewToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.partialView, this.clickDeleteAndConfirmButton(), ConstantHelper.statusCodes.ok);
  }

  async createPartialViewFolderAndWaitForPartialViewToBeCreated(folderName: string) {
    await this.clickCreateOptionsActionMenuOption();
    await this.click(this.newFolderThreeDots);
    await this.enterFolderName(folderName);
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.partialViewFolder, this.clickConfirmCreateFolderButton(), ConstantHelper.statusCodes.created);
  }

  async renameAndWaitForPartialViewToBeRenamed(newName: string) {
    await this.clickRenameActionMenuOption();
    await this.waitForTimeout(ConstantHelper.wait.medium); // Wait to make sure the partial view name is ready to be entered
    await this.enterText(this.newNameTxt, newName, {clearFirst: true, verify: true});
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.partialView, this.click(this.renameModalBtn), ConstantHelper.statusCodes.ok);
  }
}