import {Page, Locator} from "@playwright/test"
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class LanguageUiHelper extends UiBaseLocators {
  private readonly languagesMenu: Locator;
  private readonly languageDropdown: Locator;
  private readonly defaultLanguageToggle: Locator;
  private readonly mandatoryLanguageToggle: Locator;
  private readonly addFallbackLanguageBtn: Locator;
  private readonly languageTable: Locator;
  private readonly deleteLanguageEntityAction: Locator;
  private readonly languageCreateBtn: Locator;
  private readonly settingsSidebar: Locator;

  constructor(page: Page) {
    super(page);
    this.settingsSidebar = page.getByTestId('section-sidebar:Umb.SectionSidebarMenu.Settings');
    this.languagesMenu = this.settingsSidebar.getByRole('link', {name: 'Languages'});
    this.languageDropdown = page.locator('umb-input-culture-select #expand-symbol-wrapper');
    this.defaultLanguageToggle = page.locator('uui-toggle').filter({hasText: /Default language/}).locator('#toggle');
    this.mandatoryLanguageToggle = page.locator('uui-toggle').filter({hasText: /Mandatory language/}).locator('#toggle');
    this.addFallbackLanguageBtn = page.locator('#add-button');
    this.languageTable = page.locator('umb-language-table-collection-view');
    this.deleteLanguageEntityAction = page.getByTestId('entity-action:Umb.EntityAction.Language.Delete');
    this.languageCreateBtn = page.getByTestId('collection-action:Umb.CollectionAction.Language.Create');
  }

  async clickLanguageCreateButton() {
    await this.click(this.languageCreateBtn);
  }

  async clickLanguagesMenu() {
    await this.click(this.languagesMenu);
  }

  async goToLanguages() {
    await this.goToSection(ConstantHelper.sections.settings);
    await this.clickLanguagesMenu();
  }

  async removeFallbackLanguageByIsoCode(isoCode: string) {
    const languageLocator = this.page.locator(`umb-entity-item-ref[id="${isoCode}"]`);
    await this.hoverAndClick(languageLocator, languageLocator.getByLabel('Remove'));
    await this.click(this.confirmToRemoveBtn);
  }

  async chooseLanguageByName(name: string) {
    await this.click(this.languageDropdown);
    await this.click(this.page.locator('umb-input-culture-select').getByText(name, {exact: true}));
  }

  async clickLanguageByName(name: string) {
    await this.click(this.languageTable.getByText(name, {exact: true}));
  }

  async isLanguageNameVisible(name: string, isVisible = true) {
    return await this.isVisible(this.languageTable.getByText(name, {exact: true}), isVisible);
  }

  async switchDefaultLanguageOption() {
    await this.click(this.defaultLanguageToggle);
  }

  async switchMandatoryLanguageOption() {
    await this.click(this.mandatoryLanguageToggle);
  }

  async clickAddFallbackLanguageButton() {
    await this.click(this.addFallbackLanguageBtn);
  }

  async clickRemoveLanguageByName(name: string) {
    await this.click(this.page.locator('uui-table-row').filter({has: this.page.getByText(name, {exact: true})}).locator(this.deleteLanguageEntityAction), {force: true});
  }

  async removeLanguageByName(name: string) {
    await this.clickRemoveLanguageByName(name);
    await this.clickConfirmToDeleteButton();
  }

  async removeLanguageByNameAndWaitForLanguageToBeDeleted(name: string) {
    await this.clickRemoveLanguageByName(name);
    return await this.clickConfirmToDeleteButtonAndWaitForLanguageToBeDeleted();
  }

  async selectFallbackLanguageByName(name: string) {
    await this.click(this.page.locator('umb-language-picker-modal').getByLabel(name));
    await this.clickSubmitButton();
  }

  async clickSaveButtonAndWaitForLanguageToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.language, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForLanguageToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.language, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForLanguageToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.language, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }
}