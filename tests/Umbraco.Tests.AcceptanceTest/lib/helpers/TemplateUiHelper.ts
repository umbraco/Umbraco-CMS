import {Page, Locator} from "@playwright/test"
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class TemplateUiHelper extends UiBaseLocators {
  private readonly changeMasterTemplateBtn: Locator;
  private readonly sectionsBtn: Locator;
  private readonly removeMasterTemplateBtn: Locator;
  private readonly sectionNameTxt: Locator;
  private readonly templateTree: Locator;

  constructor(page: Page) {
    super(page);
    this.changeMasterTemplateBtn = page.locator('#master-template-button');
    this.sectionsBtn = page.locator('#sections-button', {hasText: 'Sections'});
    this.removeMasterTemplateBtn = page.locator('[name="icon-delete"] svg');
    this.sectionNameTxt = page.getByLabel('Section Name');
    this.templateTree = page.locator('umb-tree[alias="Umb.Tree.Template"]');
  }

  async clickActionsMenuForTemplate(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async clickActionsMenuAtRoot() {
    await this.clickActionsMenuForTemplate('Templates');
  }

  async clickRootFolderCaretButton() {
    await this.openCaretButtonForName('Templates');
  }

  async goToTemplate(templateName: string, childTemplateName: string = '') {
    await this.goToSection(ConstantHelper.sections.settings);
    await this.reloadTemplateTree();
    if (childTemplateName === '') {
      await this.click(this.page.getByLabel(templateName, {exact: true}));
      await this.hasValue(this.enterAName, templateName);
    } else {
      await this.openCaretButtonForName(templateName);
      await this.click(this.page.getByLabel(childTemplateName, {exact: true}));
      await this.hasValue(this.enterAName, childTemplateName);
    }
    await this.page.waitForTimeout(ConstantHelper.wait.medium);
  }

  async clickSectionsButton() {
    await this.click(this.sectionsBtn);
  }

  async clickChangeMasterTemplateButton() {
    await this.click(this.changeMasterTemplateBtn);
  }

  async enterTemplateName(templateName: string) {
    await this.enterText(this.enterAName, templateName);
  }

  async enterTemplateContent(templateContent: string) {
    // We need this wait, to be sure that the template content is loaded.
    await this.page.waitForTimeout(ConstantHelper.wait.minimal);
    await this.enterMonacoEditorValue(templateContent);
  }

  async isMasterTemplateNameVisible(templateName: string, isVisible: boolean = true) {
    await this.isVisible(this.page.getByLabel(`Master template: ${templateName}`), isVisible);
  }

  async clickRemoveMasterTemplateButton() {
    await this.click(this.removeMasterTemplateBtn);
  }

  async insertSection(sectionType: string, sectionName: string = '') {
    await this.clickSectionsButton();
    await this.waitForVisible(this.submitBtn);
    await this.click(this.page.locator(`[label="${sectionType}"]`));
    if (sectionName !== '') {
      await this.waitForVisible(this.sectionNameTxt);
      await this.sectionNameTxt.fill(sectionName);
    }
    await this.clickSubmitButton();
  }

  async isTemplateTreeItemVisible(templateName: string, isVisible: boolean = true) {
    await this.isVisible(this.templateTree.getByText(templateName, {exact: true}), isVisible);
  }

  async reloadTemplateTree() {
    await this.reloadTree('Templates');
  }

  async isTemplateRootTreeItemVisible(templateName: string, isVisible: boolean = true, toReload: boolean = true) {
    if (toReload) {
      await this.reloadTemplateTree();
    }
    await this.isVisible(this.templateTree.getByText(templateName, {exact: true}), isVisible);
  }

  async clickSaveButtonAndWaitForTemplateToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.template, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForTemplateToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.template, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForTemplateToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.template, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async clickDeleteAndConfirmButtonAndWaitForTemplateToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.template, this.clickDeleteAndConfirmButton(), ConstantHelper.statusCodes.ok);
  }
}