import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class BackofficeSearchUiHelper extends UiBaseLocators {
  private readonly searchHeaderBtn: Locator;
  private readonly searchModal: Locator;
  private readonly searchInputTxt: Locator;
  private readonly activeProvider: Locator;
  private readonly results: Locator;
  private readonly activeResult: Locator;
  private readonly noResults: Locator;
  private readonly navigationTips: Locator;

  constructor(page: Page) {
    super(page);
    this.searchHeaderBtn = page.getByTestId('header-app:Umb.HeaderApp.Search');
    this.searchModal = page.locator('umb-search-modal');
    this.searchInputTxt = this.searchModal.locator('input[name="search-input"]');
    this.activeProvider = this.searchModal.locator('.search-provider.active');
    this.results = this.searchModal.locator('.search-item');
    this.activeResult = this.searchModal.locator('.search-item.active');
    this.noResults = this.searchModal.locator('#no-results');
    this.navigationTips = this.searchModal.locator('#navigation-tips');
  }

  async goToSection(sectionName: string, checkSections = true, skipReload = false) {
    await super.goToSection(sectionName, checkSections, skipReload);
    await this.waitForVisible(this.activeSectionLink.getByText(sectionName));
  }

  async clickSearchHeaderButton() {
    await this.waitForPageLoad();
    await expect(async () => {
      if (!(await this.searchModal.isVisible())) {
        await this.click(this.searchHeaderBtn);
      }
      await expect(this.searchModal).toBeVisible({
        timeout: ConstantHelper.timeout.short,
      });
    }).toPass({timeout: ConstantHelper.timeout.long});
  }

  async clickOutsideToCloseModal() {
    await this.page.mouse.click(0, 0);
  }

  async isSearchModalVisible(isVisible: boolean = true) {
    await this.isVisible(this.searchModal, isVisible, ConstantHelper.timeout.long);
  }

  async enterSearchQuery(query: string) {
    await this.enterText(this.searchInputTxt, query);
  }

  async clearSearchQuery() {
    await this.searchInputTxt.fill('');
  }

  async searchForDocument(query: string) {
    await this.searchForQuery(query, ConstantHelper.apiEndpoints.documentSearch);
  }

  async searchForMedia(query: string) {
    await this.searchForQuery(query, ConstantHelper.apiEndpoints.mediaSearch);
  }

  async searchForMember(query: string) {
    await this.searchForQuery(query, ConstantHelper.apiEndpoints.memberSearch);
  }

  async searchForElement(query: string) {
    await this.searchForQuery(query, ConstantHelper.apiEndpoints.elementSearch);
  }

  async clickSearchProvider(providerName: string) {
    await this.click(this.providerByName(providerName));
  }

  async clickSearchProviderAndWaitForRerun(providerName: string, searchUrl: string) {
    await this.waitForResponseAfterExecutingPromise(
      searchUrl,
      this.click(this.providerByName(providerName)),
      ConstantHelper.statusCodes.ok,
    );
  }

  private async searchForQuery(query: string, searchUrl: string) {
    await this.waitForResponseAfterExecutingPromise(searchUrl, this.enterSearchQuery(query), ConstantHelper.statusCodes.ok);
  }

  async isSearchProviderActive(providerName: string) {
    await this.hasText(this.activeProvider, providerName);
  }
  
  async doesSearchResultHaveCount(count: number) {
    expect(await this.results.count()).toBe(count);
  }

  async isSearchProviderVisible(providerName: string, isVisible: boolean = true) {
    await this.isVisible(this.providerByName(providerName), isVisible);
  }

  async isSearchResultWithNameVisible(name: string, isVisible: boolean = true) {
    await this.isVisible(this.resultByName(name), isVisible);
  }

  async getSearchResultsCount() {
    return await this.results.count();
  }

  async clickSearchResult(name: string) {
    await this.click(this.resultByName(name));
  }

  async isNoResultsMessageVisible(isVisible: boolean = true) {
    await this.isVisible(this.noResults, isVisible);
  }

  async isNavigationTipsVisible(isVisible: boolean = true) {
    await this.isVisible(this.navigationTips, isVisible);
  }

  async pressArrowDown() {
    await this.page.keyboard.press('ArrowDown');
  }

  async pressArrowUp() {
    await this.page.keyboard.press('ArrowUp');
  }

  async getActiveSearchResultIndex() {
    await this.hasCount(this.activeResult, 1, ConstantHelper.timeout.short);
    const index = await this.activeResult.getAttribute('data-item-index');
    return index ? parseInt(index, 10) : -1;
  }

  /**
   * A search result row, matched on a **substring** — deliberately, unlike the exact-match rule
   * for entity names in CLAUDE.md §3.
   *
   * A result renders `<span>${label} <small class="ancestors">…</small></span>`, and the data
   * source populates `ancestors` for anything below root, so the span's text is
   * `"Name Parent / Path"` — no element holds the name on its own and an exact match finds
   * nothing for a child entity. The collision the exact rule guards against also does not arise
   * here: results are scoped to the search modal and to one query, not to a tree that leftover
   * data accumulates in.
   */
  private resultByName(name: string) {
    return this.results.filter({hasText: name});
  }

  private providerByName(providerName: string) {
    return this.searchModal.getByRole('button', {name: providerName, exact: true});
  }
}
