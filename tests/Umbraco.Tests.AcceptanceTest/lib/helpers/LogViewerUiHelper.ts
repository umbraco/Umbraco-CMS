import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class LogViewerUiHelper extends UiBaseLocators {
  private readonly searchBtn: Locator;
  private readonly searchLogsTxt: Locator;
  private readonly selectLogLevelBtn: Locator;
  private readonly saveSearchHeartIcon: Locator;
  private readonly searchNameTxt: Locator;
  private readonly saveSearchBtn: Locator;
  private readonly overviewBtn: Locator;
  private readonly sortLogByTimestampBtn: Locator;
  private readonly firstLogLevelTimestamp: Locator;
  private readonly firstLogLevelMessage: Locator;
  private readonly firstLogSearchResult: Locator;
  private readonly savedSearchesBtn: Locator;
  private readonly loadingSpinner: Locator;

  constructor(page: Page) {
    super(page);
    this.searchBtn = page.locator('uui-tab').filter({hasText: 'Search'}).locator('svg');
    this.searchLogsTxt = page.getByPlaceholder('Search logs...');
    this.selectLogLevelBtn = page.getByLabel('Select log levels');
    this.saveSearchHeartIcon = page.getByLabel("Save search");
    this.searchNameTxt = page.getByLabel("Search name");
    this.saveSearchBtn = page.locator('uui-dialog-layout').getByLabel("Save search");
    this.overviewBtn = page.getByRole('tab', {name: 'Overview'});
    this.sortLogByTimestampBtn = page.getByLabel('Sort logs');
    this.firstLogLevelTimestamp = page.locator('umb-log-viewer-message #timestamp').first();
    this.firstLogLevelMessage = page.locator('umb-log-viewer-message #message').first();
    this.firstLogSearchResult = page.getByRole('group').locator('#message').first();
    this.savedSearchesBtn = page.getByLabel('Saved searches');
    this.loadingSpinner = page.locator('#empty uui-loader-circle');
  }

  async clickSearchButton() {
    await this.click(this.searchBtn);
    await this.waitForVisible(this.searchLogsTxt);
  }

  async clickOverviewButton() {
    await this.click(this.overviewBtn);
  }

  async enterSearchKeyword(keyword: string) {
    await this.enterText(this.searchLogsTxt, keyword);
  }

  async selectLogLevel(level: string) {
    // The force click is necessary.
    await this.click(this.selectLogLevelBtn, {force: true});
    const logLevelLocator = this.page.locator('.log-level-menu-item').getByText(level);
    // Force click is needed
    await this.click(logLevelLocator, {force: true});
  }

  async doesLogLevelIndicatorDisplay(level: string) {
    return await this.isVisible(this.page.locator('.log-level-button-indicator', {hasText: level}));
  }

  async doesLogLevelCountMatch(level: string, expectedNumber: number) {
    await this.hasCount(this.page.locator('umb-log-viewer-message').locator('umb-log-viewer-level-tag', {hasText: level}), expectedNumber);
  }

  async saveSearch(searchName: string) {
    // The force click is necessary.
    await this.click(this.saveSearchHeartIcon, {force: true});
    await this.enterText(this.searchNameTxt, searchName);
    await this.click(this.saveSearchBtn);
  }

  checkSavedSearch(searchName: string) {
    return this.page.locator('#saved-searches').getByLabel(searchName, {exact: true});
  }

  async clickSortLogByTimestampButton() {
    await this.click(this.sortLogByTimestampBtn);
  }

  async doesFirstLogHaveTimestamp(timestamp: string) {
    await this.containsText(this.firstLogLevelTimestamp, timestamp);
  }

  async clickPageNumber(pageNumber: number) {
    await this.click(this.page.getByLabel(`Go to page ${pageNumber}`, {exact: true}));
  }

  async doesFirstLogHaveMessage(message: string) {
    await this.containsText(this.firstLogLevelMessage, message, 10000);
  }

  async clickSavedSearchByName(name: string) {
    await this.click(this.page.locator('#saved-searches').getByLabel(name));
  }

  async doesSearchBoxHaveValue(searchValue: string) {
    await expect(this.page.getByPlaceholder('Search logs...')).toHaveValue(searchValue);
  }

  async clickFirstLogSearchResult() {
    await this.click(this.firstLogSearchResult);
  }

  async doesDetailedLogHaveText(text: string) {
    await this.isVisible(this.page.locator('details[open] .property-value').getByText(text));
  }

  async clickSavedSearchesButton() {
    // The force click is necessary.
    await this.click(this.savedSearchesBtn, {force: true});
  }

  async removeSavedSearchByName(name: string) {
    const removedSavedSearchWithNameLocator = this.page.locator('.saved-search-item').filter({hasText: name}).getByLabel('Delete this search');
    // The force click is necessary.
    await this.click(removedSavedSearchWithNameLocator, {force: true});
  }

  async waitUntilLoadingSpinnerInvisible() {
    await this.hasCount(this.loadingSpinner, 0);
  }
}
