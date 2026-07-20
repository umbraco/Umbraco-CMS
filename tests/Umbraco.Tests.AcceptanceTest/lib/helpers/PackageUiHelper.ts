import {Locator, Page,} from "@playwright/test"
import {UiBaseLocators} from "./UiBaseLocators";
import {umbracoConfig} from "../umbraco.config";
import {ConstantHelper} from "./ConstantHelper";

export class PackageUiHelper extends UiBaseLocators {
  private readonly createdTabBtn: Locator;
  private readonly marketPlaceIFrame: Locator;
  private readonly installedTabBtn: Locator;
  private readonly packagesTabBtn: Locator;
  private readonly createPackageBtn: Locator;
  private readonly packageNameTxt: Locator;
  private readonly saveChangesToPackageBtn: Locator;
  private readonly addContentToPackageBtn: Locator;
  private readonly addMediaToPackageBtn: Locator;
  private readonly addDocumentTypeToPackageBtn: Locator;
  private readonly addMediaTypeToPackageBtn: Locator;
  private readonly addLanguageToPackageBtn: Locator;
  private readonly addDictionaryToPackageBtn: Locator;
  private readonly addDataTypesToPackageBtn: Locator;
  private readonly addTemplatesToPackagesBtn: Locator;
  private readonly addPartialViewToPackageBtn: Locator;
  private readonly addScriptToPackageBtn: Locator;
  private readonly addStylesheetToPackageBtn: Locator;
  private readonly downloadPackageBtn: Locator;
  private readonly propertyLayout: Locator;
  private readonly umbracoBackofficePackage: Locator;
  private readonly viewsId: Locator;

  constructor(page: Page) {
    super(page);
    this.viewsId = page.locator('#views');
    // Packages
    this.packagesTabBtn = this.viewsId.getByRole('tab', { name: 'Packages' });
    this.marketPlaceIFrame = page.frameLocator('iframe[title="Umbraco Marketplace"]').locator('umb-market-app');
    // Installed
    this.installedTabBtn = this.viewsId.getByRole('tab', { name: 'Installed' });
    // Created
    this.propertyLayout = page.locator('umb-property-layout');
    this.createdTabBtn = this.viewsId.getByRole('tab', { name: 'Created' });
    this.createPackageBtn = page.getByLabel("Create package");
    this.packageNameTxt = page.getByLabel('Name of the package');
    this.saveChangesToPackageBtn = page.getByLabel('Save changes to package');
    this.addContentToPackageBtn = page.locator('umb-input-document').getByLabel('Choose');
    this.addMediaToPackageBtn = page.locator('umb-input-media').getByLabel('Choose');
    this.addDocumentTypeToPackageBtn = this.propertyLayout.filter({hasText: 'Document Types'}).getByLabel('Choose');
    this.addMediaTypeToPackageBtn = this.propertyLayout.filter({hasText: 'Media Types'}).getByLabel('Choose');
    this.addLanguageToPackageBtn = this.propertyLayout.filter({hasText: 'Languages'}).getByLabel('Choose');
    this.addDictionaryToPackageBtn = this.propertyLayout.filter({hasText: 'Dictionary'}).getByLabel('Choose');
    this.addDataTypesToPackageBtn = this.propertyLayout.filter({hasText: 'Data Types'}).getByLabel('Choose');
    this.addTemplatesToPackagesBtn = this.propertyLayout.filter({hasText: 'Templates'}).getByLabel('Choose');
    this.addPartialViewToPackageBtn = this.propertyLayout.filter({hasText: 'Partial Views'}).getByLabel('Choose');
    this.addScriptToPackageBtn = this.propertyLayout.filter({hasText: 'Scripts'}).getByLabel('Choose');
    this.addStylesheetToPackageBtn = this.propertyLayout.filter({hasText: 'Stylesheets'}).getByLabel('Choose');
    this.downloadPackageBtn = page.getByLabel('Download');
    this.umbracoBackofficePackage = page.locator('uui-ref-node-package', {hasText: '@umbraco-cms/backoffice'});
  }

  async isUmbracoBackofficePackageVisible(isVisible = true) {
    await this.isVisible(this.umbracoBackofficePackage, isVisible);
  }

  async clickCreatedTab() {
    await this.page.waitForTimeout(ConstantHelper.wait.short);
    await this.click(this.createdTabBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickInstalledTab() {
    await this.click(this.installedTabBtn);
  }

  async clickPackagesTab() {
    await this.click(this.packagesTabBtn);
  }

  async clickChooseBtn() {
    await this.click(this.chooseModalBtn);
  }

  async isMarketPlaceIFrameVisible(isVisible = true) {
    await this.isVisible(this.marketPlaceIFrame, isVisible);
  }

  async clickCreatePackageButton() {
    await this.click(this.createPackageBtn);
  }

  async enterPackageName(packageName: string) {
    await this.enterText(this.packageNameTxt, packageName);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async isPackageNameVisible(packageName: string, isVisible = true) {
    return await this.isVisible(this.page.getByRole('button', {name: packageName}), isVisible);
  }

  async clickExistingPackageName(packageName: string) {
    await this.click(this.page.getByRole('button', {name: packageName}));
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickDeleteButtonForPackageName(packageName: string) {
    const deletePackageWithNameLocator = this.page.locator('uui-ref-node-package', {hasText: packageName}).getByLabel('Delete');
    await this.click(deletePackageWithNameLocator);
  }

  async clickSaveChangesToPackageButton() {
    await this.click(this.saveChangesToPackageBtn);
  }

  async clickAddContentToPackageButton() {
    await this.click(this.addContentToPackageBtn);
  }

  async clickAddMediaToPackageButton() {
    await this.click(this.addMediaToPackageBtn);
  }

  async clickAddDocumentTypeToPackageButton() {
    await this.click(this.addDocumentTypeToPackageBtn);
  }

  async clickAddMediaTypeToPackageButton() {
    await this.click(this.addMediaTypeToPackageBtn);
  }

  async clickAddLanguageToPackageButton() {
    await this.click(this.addLanguageToPackageBtn);
  }

  async clickAddDictionaryToPackageButton() {
    await this.click(this.addDictionaryToPackageBtn);
  }

  async clickAddDataTypesToPackageButton() {
    await this.click(this.addDataTypesToPackageBtn);
  }

  async clickAddTemplatesToPackageButton() {
    await this.click(this.addTemplatesToPackagesBtn);
  }

  async clickAddPartialViewToPackageButton() {
    await this.click(this.addPartialViewToPackageBtn);
  }

  async clickAddScriptToPackageButton() {
    await this.click(this.addScriptToPackageBtn);
  }

  async clickAddStylesheetToPackageButton() {
    await this.click(this.addStylesheetToPackageBtn);
  }

  // Downloads the package and converts it to a string
  async downloadPackage(packageId: string) {
    const responsePromise = this.page.waitForResponse(`${umbracoConfig.environment.baseUrl}/umbraco/management/api/v1/package/created/${packageId}/download`);
    await this.click(this.downloadPackageBtn);
    const response = await responsePromise;
    const body = await response.body();
    return body.toString().trim();
  }
}