import {UiBaseLocators} from "./UiBaseLocators";
import {expect, Locator, Page} from "@playwright/test";
import {ConstantHelper} from "./ConstantHelper";

export class MediaUiHelper extends UiBaseLocators {
  private readonly createMediaItemBtn: Locator;
  private readonly mediaNameTxt: Locator;
  private readonly actionModalCreateBtn: Locator;
  private readonly trashBtn: Locator;
  private readonly restoreThreeDotsBtn: Locator;
  private readonly confirmEmptyRecycleBinBtn: Locator;
  private readonly mediaCreateBtn: Locator;
  private readonly mediaListHeader: Locator;
  private readonly mediaCardItemsValues: Locator;
  private readonly mediaListView: Locator;
  private readonly mediaGridView: Locator;
  private readonly mediaListNameValues: Locator;
  private readonly bulkTrashBtn: Locator;
  private readonly bulkMoveToBtn: Locator;
  private readonly mediaHeader: Locator;
  private readonly mediaHeaderActionsMenu: Locator;
  private readonly emptyRecycleBinBtn: Locator;
  private readonly mediaTreeItem: Locator;
  private readonly mediaPopoverLayout: Locator;
  private readonly mediaWorkspace: Locator;

  constructor(page: Page) {
    super(page);
    this.createMediaItemBtn = page.locator('umb-create-media-collection-action').getByLabel('Create');
    this.mediaNameTxt = page.locator('#name-input #input');
    this.actionModalCreateBtn = page.locator('#action-modal').getByLabel('Create');
    this.trashBtn = page.getByLabel(/^Trash(…)?$/);
    this.restoreThreeDotsBtn = page.getByRole('button', {name: 'Restore…'});
    this.confirmEmptyRecycleBinBtn = page.locator('#confirm').getByLabel('Empty recycle bin', {exact: true});
    this.mediaCreateBtn = this.page.locator('umb-collection-toolbar').getByLabel('Create');
    this.mediaListView = this.page.locator('umb-media-table-collection-view');
    this.mediaGridView = this.page.locator('umb-media-grid-collection-view');
    this.mediaListHeader = this.mediaListView.locator('uui-table-head-cell span');
    this.mediaCardItemsValues = this.mediaCardItems.locator('span');
    this.mediaListNameValues = this.mediaListView.locator('umb-media-table-column-name span');
    this.bulkTrashBtn = page.locator('umb-entity-bulk-action uui-button').filter({hasText: 'Trash'});
    this.bulkMoveToBtn = page.locator('umb-entity-bulk-action uui-button').filter({hasText: 'Move to'});
    this.mediaHeader = page.getByRole('heading', {name: 'Media'});
    this.mediaHeaderActionsMenu = page.locator('#header #action-modal');
    this.emptyRecycleBinBtn = page.locator('[label="Empty recycle bin"]').locator('svg');
    this.mediaTreeItem = page.locator('umb-media-tree-item');
    this.mediaPopoverLayout = page.locator('umb-popover-layout');
    this.mediaWorkspace = page.locator('umb-media-workspace-editor');
  }

  async clickCreateMediaItemButton() {
    await this.click(this.createMediaItemBtn);
  }

  async enterMediaItemName(name: string) {
    await this.enterText(this.mediaNameTxt, name, {verify: true});
  }

  async clickMediaTypeWithNameButton(mediaTypeName: string) {
    await this.click(this.page.getByLabel(mediaTypeName, {exact: true}));
  }

  async doesMediaCardsContainAmount(count: number) {
    await this.hasCount(this.mediaCardItems, count);
  }

  async doesMediaCardContainText(name: string) {
    await this.containsText(this.mediaCardItems, name);
  }

  async clickTrashButton() {
    await this.click(this.trashBtn);
  }

  async restoreMediaItem(name: string) {
    await this.clickActionsMenuForName(name);
    await this.click(this.restoreThreeDotsBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.medium);
    await this.clickRestoreButton();
  }

  async deleteMediaItem(name: string) {
    await this.clickActionsMenuForName(name);
    await this.clickDeleteActionMenuOption();
    await this.clickConfirmToDeleteButton();
  }

  async deleteMediaItemAndWaitForMediaToBeDeleted(name: string) {
    await this.clickActionsMenuForName(name);
    await this.clickDeleteActionMenuOption();
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.media, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async clickCreateMediaWithType(mediaTypeName: string) {
    await this.click(this.mediaCreateBtn);
    await this.clickMediaTypeInPopoverByName(mediaTypeName);
  }

  async clickMediaTypeName(mediaTypeName: string) {
    await this.click(this.documentTypeNode.filter({hasText: mediaTypeName}));
  }

  async clickMediaTypeInPopoverByName(mediaTypeName: string) {
    await this.click(this.mediaPopoverLayout.getByLabel(mediaTypeName));
  }

  async clickEmptyRecycleBinButton() {
    // Force click is needed
    await this.hoverAndClick(this.recycleBinMenuItem, this.emptyRecycleBinBtn, {force: true});
  }

  async clickConfirmEmptyRecycleBinButton() {
    await this.click(this.confirmEmptyRecycleBinBtn);
  }

  async clickCreateModalButton() {
    await this.click(this.actionModalCreateBtn);
  }

  async clickMediaCaretButtonForName(name: string) {
    await this.click(this.page.locator(`umb-media-tree-item [label="${name}"]`).locator('#caret-button'));
  }

  async openMediaCaretButtonForName(name: string) {
    const menuItem = this.page.locator(`umb-media-tree-item [label="${name}"]`);
    const isCaretButtonOpen = await menuItem.getAttribute('show-children');

    if (isCaretButtonOpen === null) {
      await this.clickMediaCaretButtonForName(name);
    }
  }
  
  async doesMediaGridValuesMatch(expectedValues: string[]) {
    return expectedValues.forEach((text, index) => {
      expect(this.mediaCardItemsValues.nth(index)).toHaveText(text);
    });
  }

  async doesMediaListHeaderValuesMatch(expectedValues: string[]) {
    return expectedValues.forEach((text, index) => {
      expect(this.mediaListHeader.nth(index)).toHaveText(text);
    });
  }

  async doesMediaListNameValuesMatch(expectedValues: string[]) {
    return expectedValues.forEach((text, index) => {
      expect(this.mediaListNameValues.nth(index)).toHaveText(text);
    });
  }

  async isMediaGridViewVisible(isVisible: boolean = true) {
    await this.isVisible(this.mediaGridView, isVisible);
  }

  async isMediaListViewVisible(isVisible: boolean = true) {
    await this.isVisible(this.mediaListView, isVisible);
  }

  async doesMediaWorkspaceHaveText(text: string) {
    await this.containsText(this.mediaWorkspace, text);
  }

  async clickBulkTrashButton() {
    await this.click(this.bulkTrashBtn);
  }

  async clickBulkMoveToButton() {
    await this.click(this.bulkMoveToBtn);
  }

  async clickModalTextByName(name: string) {
    await this.click(this.sidebarModal.getByLabel(name, {exact: true}));
  }

  async reloadMediaTree() {
    await this.click(this.mediaHeader);
    await this.click(this.mediaHeaderActionsMenu, {force: true});
    await this.clickReloadChildrenActionMenuOption();
  }

  async isMediaTreeItemVisible(name: string, isVisible: boolean = true) {
    return await this.isVisible(this.mediaTreeItem.getByLabel(name, {exact: true}), isVisible);
  }

  async doesMediaItemInTreeHaveThumbnail(name: string, thumbnailIconName: string) {
    const mediaThumbnailIconLocator = this.page.locator(`umb-media-tree-item [label="${name}"]`).locator('#icon-container #icon');
    await this.hasAttribute(mediaThumbnailIconLocator, 'name', thumbnailIconName);
  }

  async isChildMediaVisible(parentName: string, childName: string, isVisible: boolean = true) {
    return await this.isVisible(this.mediaTreeItem.filter({hasText: parentName}).getByText(childName, {exact: true}), isVisible);
  }

  async clickCaretButtonForMediaName(name: string) {
    await this.click(this.mediaTreeItem.filter({hasText: name}).last().locator('#caret-button').last());
  }

  async goToMediaWithName(mediaName: string) {
    await this.click(this.mediaTreeItem.getByText(mediaName, {exact: true}));
  }

  async clickSaveButtonAndWaitForMediaToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.media, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForMediaToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.media, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmTrashButtonAndWaitForMediaToBeTrashed() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.media, this.clickConfirmTrashButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmEmptyRecycleBinButtonAndWaitForRecycleBinToBeEmptied() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.recycleBinMedia, this.clickConfirmEmptyRecycleBinButton(), ConstantHelper.statusCodes.ok);
  }

  async clickChooseModalButtonAndWaitForMediaItemsToBeMoved(movedMediaItems: number) {
    return await this.waitForMultipleResponsesAfterExecutingPromise('/move', this.clickChooseModalButton(), 200, movedMediaItems);
  }
}