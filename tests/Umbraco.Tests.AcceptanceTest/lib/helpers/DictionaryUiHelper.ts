import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class DictionaryUiHelper extends UiBaseLocators {
  private readonly createDictionaryItemBtn: Locator;
  private readonly dictionaryNameTxt: Locator;
  private readonly exportBtn: Locator;
  private readonly includeDescendantsCheckbox: Locator;
  private readonly importBtn: Locator;
  private readonly importFileTxt: Locator;
  private readonly emptySearchResultMessage: Locator;
  private readonly dictionaryList: Locator;
  private readonly dictionaryListRows: Locator;
  private readonly dictionaryTree: Locator;
  private readonly dictionaryCollection: Locator;
  private readonly exportModalBtn: Locator;
  private readonly importModalBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.createDictionaryItemBtn = page.getByLabel('Create dictionary item', {exact: true});
    this.dictionaryNameTxt = page.locator('umb-workspace-header-name-editable').locator('input');
    this.exportBtn = page.getByRole('button', {name: /^Export(…)?$/});
    this.importBtn = page.getByRole('button', {name: /^Import(…)?$/});
    this.dictionaryList = page.locator('umb-dictionary-table-collection-view');
    this.dictionaryListRows = this.dictionaryList.locator('uui-table-row');
    this.exportModalBtn = page.locator('umb-export-dictionary-modal').getByLabel('Export');
    this.includeDescendantsCheckbox = page.locator('umb-export-dictionary-modal #includeDescendants');
    this.importModalBtn = page.locator('umb-import-dictionary-modal').locator('uui-button').filter({hasText: 'Import'}).getByLabel('Import');
    this.importFileTxt = page.locator('umb-import-dictionary-modal #input');
    this.emptySearchResultMessage = page.locator('#empty-state');
    this.dictionaryTree = page.locator('umb-tree[alias="Umb.Tree.Dictionary"]');
    this.dictionaryCollection = page.locator('umb-dictionary-collection');
  }

  async clickCreateDictionaryItemButton() {
    await this.click(this.createDictionaryItemBtn);
  }

  async enterDictionaryName(name: string) {
    await this.enterText(this.dictionaryNameTxt, name);
  }

  async clickActionsMenuForDictionary(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async clickExportButton() {
    await this.click(this.exportBtn);
  }

  async clickImportButton() {
    await this.click(this.importBtn);
  }

  async deleteDictionary() {
    await this.clickDeleteActionMenuOption();
    await this.click(this.confirmToDeleteBtn);
  }

  async doesDictionaryListHaveText(text: string) {
    await this.waitForVisible(this.dictionaryList);
    const allRows = await this.dictionaryListRows.all();
    for (let currentRow of allRows) {
      const currentText = await currentRow.innerText();
      if (currentText.includes(text)) {
        return true;
      }
    }
    return false;
  }

  // This function will export dictionary and return the file name
  async exportDictionary(includesDescendants: boolean) {
    if (includesDescendants) {
      await this.click(this.includeDescendantsCheckbox);
    }
    const [downloadPromise] = await Promise.all([
      this.page.waitForEvent('download'),
      await this.click(this.exportModalBtn)
    ]);
    return downloadPromise.suggestedFilename();
  }

  async importDictionary(filePath: string) {
    await this.importFileTxt.setInputFiles(filePath);
    await this.click(this.importModalBtn);
  }

  async isSearchResultMessageDisplayEmpty(message: string) {
    await this.hasText(this.emptySearchResultMessage, message);
  }

  async isDictionaryTreeItemVisible(dictionaryName: string, isVisible: boolean = true) {
    await this.isVisible(this.dictionaryTree.getByText(dictionaryName, {exact: true}), isVisible);
  }

  async doesDictionaryCollectionContainText(text: string) {
    await this.containsText(this.dictionaryCollection, text);
  }

  async clickSaveButtonAndWaitForDictionaryToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dictionary, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForDictionaryToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dictionary, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForDictionaryToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dictionary, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async deleteDictionaryAndWaitForDictionaryToBeDeleted() {
    await this.clickDeleteActionMenuOption();
    return await this.clickConfirmToDeleteButtonAndWaitForDictionaryToBeDeleted();
  }

  async importDictionaryAndWaitForDictionaryToBeImported(filePath: string) {
    await this.importFileTxt.setInputFiles(filePath);
    await this.waitForVisible(this.importModalBtn);
    await this.waitForEnabled(this.importModalBtn);
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dictionaryImport, this.click(this.importModalBtn), ConstantHelper.statusCodes.created);
  }
}