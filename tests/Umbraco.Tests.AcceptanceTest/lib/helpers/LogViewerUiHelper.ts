import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

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
  private readonly logTimestamps: Locator;
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
    this.logTimestamps = page.locator('umb-log-viewer-message #timestamp');
    this.firstLogLevelMessage = page.locator('umb-log-viewer-message #message').first();
    this.firstLogSearchResult = page.getByRole('group').locator('#message').first();
    this.savedSearchesBtn = page.getByLabel('Saved searches');
    this.loadingSpinner = page.locator('#empty uui-loader-circle');
  }

  async clickSearchButton() {
    // Also wait for the view's own initial log fetch: the frontend doesn't cancel/sequence requests, so an
    // action performed immediately after opening Search (e.g. sorting) can fire a second request that
    // resolves before this one, and the initial (default, descending) response then overwrites it.
    await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.logViewerLog, this.click(this.searchBtn), ConstantHelper.statusCodes.ok);
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
    // Exact match so a longer-named sibling can't satisfy the negative (.not.toBeVisible) assertion.
    return this.page.locator('.saved-search-item').filter({has: this.page.getByText(searchName, {exact: true})});
  }

  async clickSortLogByTimestampButton(orderDirection: 'Ascending' | 'Descending' = 'Ascending') {
    // The log viewer polls this endpoint on its own timer, so a generic endpoint match can resolve on an
    // unrelated poll response instead of the one this toggle actually triggered - match the requested
    // direction specifically.
    return await this.waitForResponseAfterExecutingPromise(`${ConstantHelper.apiEndpoints.logViewerLog}?skip=0&take=100&orderDirection=${orderDirection}`, this.click(this.sortLogByTimestampBtn), ConstantHelper.statusCodes.ok, ConstantHelper.httpMethods.get);
  }

  async doesFirstLogHaveTimestamp(timestamp: string) {
    await this.containsText(this.firstLogLevelTimestamp, timestamp);
  }

  async getLogTimestamps() {
    await this.waitForVisible(this.firstLogLevelTimestamp);
    return await this.logTimestamps.allInnerTexts();
  }

  async clickPageNumber(pageNumber: number) {
    await this.click(this.page.getByLabel(`Go to page ${pageNumber}`, {exact: true}));
  }

  async doesFirstLogHaveMessage(message: string) {
    await this.containsText(this.firstLogLevelMessage, message, 10000);
  }

  async getFirstLogMessage() {
    await this.waitForVisible(this.firstLogLevelMessage);
    return await this.firstLogLevelMessage.innerText();
  }

  async clickSavedSearchByName(name: string) {
    await this.clickSavedSearchesButton();
    // Click the item's search button (the <li> also holds a delete button); clicking the button is what
    // applies the saved query.
    await this.click(this.checkSavedSearch(name).locator('.saved-search-item-button').first());
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
