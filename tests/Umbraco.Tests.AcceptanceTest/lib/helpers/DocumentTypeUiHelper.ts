import {UiBaseLocators} from "./UiBaseLocators";
import {Locator, Page} from "@playwright/test";
import {ConstantHelper} from "./ConstantHelper";

export class DocumentTypeUiHelper extends UiBaseLocators {
  private readonly newDocumentTypeBtn: Locator;
  private readonly sharedAcrossCulturesToggle: Locator;
  private readonly documentTypeSettingsTabBtn: Locator;
  private readonly documentTypeTemplatesTabBtn: Locator;
  private readonly varyBySegmentsBtn: Locator;
  private readonly varyByCultureBtn: Locator;
  private readonly createDocumentTypeBtn: Locator;
  private readonly createDocumentTypeWithTemplateBtn: Locator;
  private readonly createElementTypeBtn: Locator;
  private readonly createDocumentFolderBtn: Locator;
  private readonly preventCleanupBtn: Locator;
  private readonly setAsDefaultBtn: Locator;
  private readonly tabGroup: Locator;
  private readonly documentTypesMenu: Locator;
  private readonly createDocumentModal: Locator;

  constructor(page: Page) {
    super(page);
    this.createDocumentModal = page.locator('umb-entity-create-option-action-list-modal');
    this.newDocumentTypeBtn = page.getByLabel('New Document Type…');
    this.sharedAcrossCulturesToggle = page.locator('label').filter({hasText: 'Shared across cultures'}).locator('#toggle');
    this.tabGroup = page.getByTestId('workspace:view-links');
    this.documentTypeSettingsTabBtn = this.tabGroup.locator('[data-mark*="Settings"]');
    this.documentTypeTemplatesTabBtn = this.tabGroup.locator('[data-mark*="Templates"]');
    this.varyBySegmentsBtn = page.getByText('Vary by segment', {exact: true});
    this.varyByCultureBtn = page.getByText('Vary by culture', {exact: true});
    this.createDocumentTypeBtn = this.createDocumentModal.locator('umb-ref-item').getByText('Document Type', {exact: true});
    this.createDocumentTypeWithTemplateBtn = this.createDocumentModal.locator('umb-ref-item', {hasText: 'Document Type with template'});
    this.createElementTypeBtn = this.createDocumentModal.locator('umb-ref-item', {hasText: 'Element Type'});
    this.createDocumentFolderBtn = this.createDocumentModal.locator('umb-ref-item', {hasText: 'Folder'});
    this.preventCleanupBtn = page.getByText('Prevent clean up');
    this.setAsDefaultBtn = page.getByText('Set as default');
    this.documentTypesMenu = page.locator('#menu-item').getByRole('link', {name: 'Document Types'});
  }

  async clickActionsMenuForDocumentType(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async clickActionsMenuAtRoot() {
    await this.clickActionsMenuForDocumentType("Document Types");
  }

  async clickRootFolderCaretButton() {
    await this.openCaretButtonForName('Document Types');
  }

  async clickNewDocumentTypeButton() {
    await this.click(this.newDocumentTypeBtn);
  }

  async clickSharedAcrossCulturesToggle() {
    await this.click(this.sharedAcrossCulturesToggle);
  }

  async clickDocumentTypeSettingsTab() {
    await this.click(this.documentTypeSettingsTabBtn);
  }

  async clickDocumentTypeTemplatesTab() {
    await this.click(this.documentTypeTemplatesTabBtn);
  }

  async clickVaryBySegmentsButton() {
    await this.click(this.varyBySegmentsBtn);
  }

  async clickVaryByCultureButton() {
    await this.click(this.varyByCultureBtn);
  }

  async clickPreventCleanupButton() {
    await this.click(this.preventCleanupBtn);
  }

  async reloadDocumentTypeTree() {
    await this.reloadTree('Document Types');
  }

  async goToDocumentType(documentTypeName: string) {
    await this.clickRootFolderCaretButton();
    await this.clickLabelWithName(documentTypeName);
  }

  async enterDocumentTypeName(documentTypeName: string) {
    await this.enterText(this.enterAName, documentTypeName, {verify: true});
  }

  async clickCreateDocumentTypeButton() {
    await this.click(this.createDocumentTypeBtn);
  }

  async clickCreateDocumentTypeWithTemplateButton() {
    await this.click(this.createDocumentTypeWithTemplateBtn);
  }

  async clickCreateElementTypeButton() {
    await this.click(this.createElementTypeBtn);
  }

  async clickCreateDocumentFolderButton() {
    await this.click(this.createDocumentFolderBtn);
  }

  async isDocumentTreeItemVisible(name: string, isVisible = true) {
    const documentTreeItem = this.page.locator('umb-tree-item').locator(`[label="${name}"]`);
    await this.isVisible(documentTreeItem, isVisible);
  }

  async clickSetAsDefaultButton() {
    await this.click(this.setAsDefaultBtn);
  }

  async clickDocumentTypesMenu() {
    await this.click(this.documentTypesMenu);
  }

  async clickSaveButtonAndWaitForDocumentTypeToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentType, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForDocumentTypeAndTemplateToBeCreated() {
    const [documentTypeId, templateId] = await this.waitForCreatedResponsesAfterExecutingPromise(
      ['/document-type', '/template'],
      this.clickSaveButton()
    );
    return { documentTypeId, templateId };
  }

  async clickSaveButtonAndWaitForDocumentTypeToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentType, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForDocumentTypeToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentType, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async clickDeleteAndConfirmButtonAndWaitForDocumentTypeToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentType, this.clickDeleteAndConfirmButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmCreateFolderButtonAndWaitForDocumentTypeToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentTypeFolder, this.clickConfirmCreateFolderButton(), ConstantHelper.statusCodes.created);
  }

  async clickCreateFolderButtonAndWaitForDocumentTypeToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentTypeFolder, this.clickCreateFolderButton(), ConstantHelper.statusCodes.created);
  }

  async clickConfirmRenameButtonAndWaitForDocumentTypeToBeRenamed() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentTypeFolder, this.clickConfirmRenameButton(), ConstantHelper.statusCodes.ok);
  }
}