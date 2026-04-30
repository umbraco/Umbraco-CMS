import {Page, Locator} from "@playwright/test"
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class DocumentBlueprintUiHelper extends UiBaseLocators{
  private readonly documentBlueprintTree: Locator;
  private readonly documentBlueprintNameTxt: Locator;
  private readonly deleteMenu: Locator;

  constructor(page: Page) {
    super(page);
    this.documentBlueprintTree = page.locator('umb-tree[alias="Umb.Tree.DocumentBlueprint"]');
    this.documentBlueprintNameTxt = page.locator('#name-input #input');
    this.deleteMenu = page.locator('umb-section-sidebar #menu-item').getByLabel('Delete');
  }

  async clickActionsMenuForDocumentBlueprints(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async clickActionsMenuAtRoot() {
    await this.clickActionsMenuForDocumentBlueprints('Document Blueprints');
  }

  async clickRootFolderCaretButton() {
    await this.openCaretButtonForName('Document Blueprints');
  }

  async clickSaveButtonAndWaitForDocumentBlueprintToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentBlueprint, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForDocumentBlueprintToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentBlueprint, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }
  
  async reloadDocumentBlueprintsTree() {
    await this.reloadTree('Document Blueprints');
  }
  
  async goToDocumentBlueprint(blueprintName: string) {
    await this.goToSection(ConstantHelper.sections.settings);
    await this.reloadDocumentBlueprintsTree();
    await this.click(this.page.getByLabel(blueprintName, {exact: true}));
  }

  async isDocumentBlueprintRootTreeItemVisible(blueprintName: string, isVisible: boolean = true, toReload: boolean = true){
    if (toReload) {
      await this.reloadDocumentBlueprintsTree();
    }
    await this.isVisible(this.documentBlueprintTree.getByText(blueprintName, {exact: true}), isVisible);
  }

  async clickCreateDocumentBlueprintButton() {
    await this.click(this.createDocumentBlueprintBtn);
  }

  async clickCreateNewDocumentBlueprintButton() {
    await this.click(this.createNewDocumentBlueprintBtn);
  }

  async enterDocumentBlueprintName(blueprintName: string) {
    await this.enterText(this.documentBlueprintNameTxt, blueprintName);
  }

  async clickDeleteMenuButton() {
    await this.click(this.deleteMenu);
  }

  async clickConfirmToDeleteButtonAndWaitForDocumentBlueprintToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentBlueprint, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }
}