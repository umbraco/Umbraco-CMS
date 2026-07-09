import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class ExamineManagementUiHelper extends UiBaseLocators {
  private readonly examineManagementTab: Locator;
  private readonly indexersContent: Locator;
  private readonly indexerItems: Locator;
  private readonly indexInfoRow: Locator;

  constructor(page: Page) {
    super(page);
    this.examineManagementTab = page.getByRole('tab', {name: 'Examine Management'});
    this.indexersContent = page.locator('[headline="Indexers"]');
    this.indexerItems = this.indexersContent.locator('uui-table-cell a');
    this.indexInfoRow = page.locator('uui-table-row');
  }

  async clickExamineManagementTab() {
    await this.click(this.examineManagementTab);
  }

  async doesIndexersHaveText(text: string) {
    await this.containsText(this.indexersContent, text);
  }

  checkIndexersCount() {
    return this.indexerItems.count();
  }

  async clickIndexByName(indexName: string) {
    await this.click(this.page.getByRole('link', {name: indexName}));
  }

  async doesIndexPropertyHaveValue(indexProperty: string, indexValue: string) {
    return await this.hasText(this.indexInfoRow.filter({has: this.page.getByText(indexProperty)}).getByRole('cell').last(), indexValue);
  }

  async doesIndexHaveHealthStatus(indexName: string, status: string) {
    return await this.isVisible(this.page.locator(`[headline='${indexName}']`).getByText(status));
  }
}
