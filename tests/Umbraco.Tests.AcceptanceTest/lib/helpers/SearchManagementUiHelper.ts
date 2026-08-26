import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class SearchManagementUiHelper extends UiBaseLocators {
  private readonly indexTableRows: Locator;
  private readonly reloadIndexListBtn: Locator;
  private readonly statsBox: Locator;
  private readonly statsBoxHealthTag: Locator;
  private readonly rebuildConfirmBtn: Locator;
  private readonly warningNotification: Locator;
  private readonly searchBox: Locator;
  private readonly searchInputTxt: Locator;
  private readonly searchSubmitBtn: Locator;
  private readonly searchResultsTable: Locator;
  private readonly searchNoResultsMessage: Locator;
  private readonly searchPagination: Locator;
  private readonly searchCultureSelect: Locator;

  constructor(page: Page) {
    super(page);
    this.indexTableRows = page.locator('umb-search-root-collection-view').locator('umb-table').locator('uui-table-row');
    this.reloadIndexListBtn = page.getByTestId('collection-action:Umbraco.Search.CollectionAction.Reload');
    this.statsBox = page.locator('umb-search-index-stats-box');
    this.statsBoxHealthTag = this.statsBox.locator('uui-tag');
    this.rebuildConfirmBtn = page.locator('#confirm').getByLabel('Rebuild Index', {exact: true});
    // The rebuild-started toast uses the 'warning' color (not 'positive' like other success toasts), since
    // it's reporting an in-progress background operation rather than a completed one.
    this.warningNotification = page.locator('uui-toast-notification[open][color="warning"]');
    this.searchBox = page.locator('umb-search-index-search-box');
    this.searchInputTxt = this.searchBox.locator('#search-input').locator('#input');
    // uui-button sets its accessible name from the `label` attribute, not its visible text - the button
    // reads "Search" on screen but is only reachable by role via its aria-label, "Execute search".
    this.searchSubmitBtn = this.searchBox.getByLabel('Execute search', {exact: true});
    this.searchResultsTable = this.searchBox.locator('umb-table');
    // getByText doesn't match here - its content lives inside umb-localize's own shadow root, not as light-DOM
    // text on this element - so target the wrapping element by its class instead.
    this.searchNoResultsMessage = this.searchBox.locator('.no-results');
    this.searchPagination = this.searchBox.locator('uui-pagination');
    this.searchCultureSelect = this.searchBox.locator('uui-select');
  }

  async goToSearchTreeItem() {
    await this.goToSettingsTreeItem('Search');
  }

  private indexRowByAlias(indexAlias: string) {
    return this.indexTableRows.filter({has: this.page.getByRole('link', {name: indexAlias, exact: true})});
  }

  async goToIndexWithAlias(indexAlias: string) {
    await this.click(this.indexRowByAlias(indexAlias).getByRole('link', {name: indexAlias, exact: true}));
    await this.waitUntilUiLoaderIsNoLongerVisible();
  }

  async isIndexRowVisible(indexAlias: string, isVisible: boolean = true) {
    await this.isVisible(this.indexRowByAlias(indexAlias), isVisible);
  }

  async doesIndexRowContainText(indexAlias: string, text: string) {
    await this.containsText(this.indexRowByAlias(indexAlias), text);
  }

  async doesIndexTableHaveColumnHeaders(headers: string[]) {
    for (const header of headers) {
      await this.isVisible(this.page.locator('umb-search-root-collection-view').getByText(header, {exact: true}));
    }
  }

  async clickRefreshListButton() {
    await this.click(this.reloadIndexListBtn);
  }

  async clickRefreshListButtonAndWaitForReload() {
    await this.waitForResponseAfterExecutingPromise(
      ConstantHelper.apiEndpoints.searchIndexes,
      this.clickRefreshListButton(),
      ConstantHelper.statusCodes.ok,
      ConstantHelper.httpMethods.get,
    );
  }

  async isStatsBoxVisible(isVisible: boolean = true) {
    await this.isVisible(this.statsBox, isVisible);
  }

  async doesStatsBoxContainText(text: string) {
    await this.containsText(this.statsBox, text);
  }

  async getStatsBoxHealthStatusText() {
    return await this.getText(this.statsBoxHealthTag);
  }

  async clickRebuildIndexEntityAction() {
    // The workspace's entity-action dropdown (data-mark="workspace:action-menu-button") is the same
    // control BasePage's actionBtn targets for a workspace's own action menu.
    await this.clickActionButton();
    await this.clickEntityActionWithName('RebuildIndex');
  }

  async doesRebuildConfirmModalHaveText(text: string) {
    await this.doesModalHaveText(text);
  }

  async clickConfirmRebuildButton() {
    await this.click(this.rebuildConfirmBtn);
  }

  async clickConfirmRebuildButtonAndWaitForResponse() {
    await this.waitForResponseAfterExecutingPromise(
      ConstantHelper.apiEndpoints.searchRebuild,
      this.clickConfirmRebuildButton(),
      ConstantHelper.statusCodes.ok,
      ConstantHelper.httpMethods.put,
    );
  }

  async doesRebuildStartedNotificationHaveText(text: string) {
    await this.containsText(this.warningNotification, text);
  }

  async enterSearchQuery(query: string) {
    await this.enterText(this.searchInputTxt, query);
  }

  async clickSearchSubmitButton() {
    // The button's slotted <umb-localize> text node sits on top of the click point and Playwright's
    // actionability check never resolves against it, so a plain click retries indefinitely.
    await this.click(this.searchSubmitBtn, {force: true});
  }

  async searchForQuery(query: string) {
    await this.enterSearchQuery(query);
    await this.clickSearchSubmitButton();
  }

  async searchForQueryAndWaitForResponse(query: string) {
    await this.enterSearchQuery(query);
    await this.waitForResponseAfterExecutingPromise(
      ConstantHelper.apiEndpoints.searchQuery,
      this.clickSearchSubmitButton(),
      ConstantHelper.statusCodes.ok,
      ConstantHelper.httpMethods.post,
    );
  }

  async isSearchResultsTableVisible(isVisible: boolean = true) {
    await this.isVisible(this.searchResultsTable, isVisible);
  }

  async isSearchNoResultsMessageVisible(isVisible: boolean = true) {
    await this.isVisible(this.searchNoResultsMessage, isVisible);
  }

  async isSearchPaginationVisible(isVisible: boolean = true) {
    await this.isVisible(this.searchPagination, isVisible);
  }

  async isSearchCultureSelectVisible(isVisible: boolean = true) {
    await this.isVisible(this.searchCultureSelect, isVisible);
  }

  async doesSearchResultsTableContainText(text: string) {
    await this.containsText(this.searchResultsTable, text);
  }
}
