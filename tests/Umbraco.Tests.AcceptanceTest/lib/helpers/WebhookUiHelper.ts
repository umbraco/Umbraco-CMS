import {Page, Locator} from "@playwright/test"
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class WebhookUiHelper extends UiBaseLocators {
  private readonly webhookCreateBtn: Locator;
  private readonly webhookNameTxt: Locator;
  private readonly urlTxt: Locator;
  private readonly chooseEventBtn: Locator;
  private readonly chooseContentTypeBtn: Locator;
  private readonly enabledToggle: Locator;
  private readonly addHeadersBtn: Locator;
  private readonly headerNameTxt: Locator;
  private readonly headerValueTxt: Locator;
  private readonly deleteWebhookEntityAction: Locator;
  private readonly headerRemoveBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.webhookCreateBtn = page.getByTestId('collection-action:Umb.CollectionAction.Webhook.Create');
    this.webhookNameTxt = page.locator('#name #input');
    this.urlTxt = page.locator('umb-property-layout[label="URL"] #input');
    this.chooseEventBtn = page.locator('umb-property-layout[label="Events"]').getByLabel('Choose');
    this.chooseContentTypeBtn = page.locator('umb-property-layout[label="Content Type"]').getByLabel('Choose');
    this.enabledToggle = page.locator('umb-property-layout[label="Enabled"] #toggle');
    this.addHeadersBtn = page.locator('umb-property-layout[label="Headers"] #add');
    this.headerNameTxt = page.locator('umb-input-webhook-headers').locator('[list="nameList"]');
    this.headerValueTxt = page.locator('umb-input-webhook-headers').locator('[list="valueList"]');
    this.deleteWebhookEntityAction = page.getByTestId('entity-action:Umb.EntityAction.Webhook.Delete');
    this.headerRemoveBtn = page.locator('umb-input-webhook-headers').locator('[label="Remove"]');
  }

  async goToWebhooks() {
    await this.goToSection(ConstantHelper.sections.settings);
    await this.goToSettingsTreeItem('Webhooks');
  }

  async goToWebhookWithName(name: string) {
    await this.goToWebhooks();
    await this.clickTextButtonWithName(name);
  }

  async clickWebhookCreateButton() {
    await this.click(this.webhookCreateBtn);
  }

  async enterWebhookName(name: string) {
    await this.enterText(this.webhookNameTxt, name);
  }

  async enterUrl(url: string) {
    await this.enterText(this.urlTxt, url);
  }

  async clickChooseEventButton() {
    await this.click(this.chooseEventBtn);
  }

  async clickChooseContentTypeButton() {
    await this.click(this.chooseContentTypeBtn);
  }

  async clickEnabledToggleButton() {
    await this.click(this.enabledToggle);
  }

  async clickAddHeadersButton() {
    await this.click(this.addHeadersBtn);
  }

  async enterHeaderName(name: string) {
    await this.enterText(this.headerNameTxt, name);
  }

  async enterHeaderValue(value: string) {
    await this.enterText(this.headerValueTxt, value);
  }

  async clickDeleteWebhookWithName(name: string) {
    const deleteLocator = this.page.locator('uui-table-row').filter({has: this.page.getByText(name, {exact: true})}).locator(this.deleteWebhookEntityAction);
    await this.click(deleteLocator);
  }

  async clickHeaderRemoveButton() {
    await this.click(this.headerRemoveBtn);
  }

  async clickSaveButtonAndWaitForWebhookToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.webhook, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForWebhookToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.webhook, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }
}