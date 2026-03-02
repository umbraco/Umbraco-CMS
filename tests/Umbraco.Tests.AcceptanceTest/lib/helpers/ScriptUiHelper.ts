import {Page, Locator} from "@playwright/test"
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class ScriptUiHelper extends UiBaseLocators{
  private readonly newJavascriptFileBtn: Locator;
  private readonly scriptTree: Locator;
  private readonly newFolderThreeDots: Locator;
  private readonly scriptCreateModal: Locator;

  constructor(page: Page) {
    super(page);
    this.scriptCreateModal = page.locator('umb-script-create-options-modal');
    this.newJavascriptFileBtn = this.scriptCreateModal.locator('umb-ref-item', {hasText: 'JavaScript file'});
    this.newFolderThreeDots = this.scriptCreateModal.locator('umb-ref-item', {hasText: 'Folder'});
    this.scriptTree = page.locator('umb-tree[alias="Umb.Tree.Script"]');
  }

  async clickActionsMenuForScript(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async createScriptFolder(folderName: string) {
    await this.clickCreateOptionsActionMenuOption();
    await this.click(this.newFolderThreeDots);
    await this.enterFolderName(folderName);
    await this.clickConfirmCreateFolderButton();
  }

  async clickActionsMenuAtRoot() {
    await this.clickActionsMenuForScript('Scripts');
  }

  async clickRootFolderCaretButton() {
    await this.openCaretButtonForName('Scripts');
  }

  async clickNewJavascriptFileButton() {
    await this.click(this.newJavascriptFileBtn);
  }
  
  async clickSaveButtonAndWaitForScriptToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.script, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForScriptToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.script, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }
  
  // Will only work for root scripts
  async goToScript(scriptName: string) {
    await this.goToSection(ConstantHelper.sections.settings);
    await this.reloadScriptTree();
    await this.click(this.page.getByLabel(scriptName, {exact: true}));
  }

  async enterScriptName(scriptContent: string) {
    await this.enterText(this.enterAName, scriptContent);
  }

  async enterScriptContent(scriptContent: string) {
    await this.enterMonacoEditorValue(scriptContent);
  }

  async openScriptAtRoot(scriptName: string) {
    await this.reloadScriptTree();
    await this.click(this.page.getByLabel(scriptName, {exact: true}));
  }

  async reloadScriptTree() {
    await this.reloadTree('Scripts');
  }

  async isScriptRootTreeItemVisible(scriptName: string, isVisible: boolean = true, toReload: boolean = true){
    if (toReload) {
      await this.reloadScriptTree();
    }
    return await this.isVisible(this.scriptTree.getByText(scriptName, {exact: true}), isVisible);
  }

  async clickConfirmToDeleteButtonAndWaitForScriptToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.script, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async clickDeleteAndConfirmButtonAndWaitForScriptToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.script, this.clickDeleteAndConfirmButton(), ConstantHelper.statusCodes.ok);
  }

  async createScriptFolderAndWaitForScriptToBeCreated(folderName: string) {
    await this.clickCreateOptionsActionMenuOption();
    await this.click(this.newFolderThreeDots);
    await this.enterFolderName(folderName);
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.scriptFolder, this.clickConfirmCreateFolderButton(), ConstantHelper.statusCodes.created);
  }

  async renameAndWaitForScriptToBeRenamed(newName: string) {
    await this.clickRenameActionMenuOption();
    await this.waitForTimeout(ConstantHelper.wait.medium); // Wait to make sure the script name is ready to be entered
    await this.enterText(this.newNameTxt, newName, {clearFirst: true, verify: true});
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.script, this.click(this.renameModalBtn), ConstantHelper.statusCodes.ok);
  }
}