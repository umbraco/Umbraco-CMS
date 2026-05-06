import {Locator, Page} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class BackofficeSearchUiHelper extends UiBaseLocators {
  private readonly searchHeaderBtn: Locator;
  private readonly searchModal: Locator;
  private readonly input: Locator;
  private readonly providers: Locator;
  private readonly activeProvider: Locator;
  private readonly results: Locator;
  private readonly activeResult: Locator;
  private readonly noResults: Locator;
  private readonly navigationTips: Locator;

  constructor(page: Page) {
    super(page);
    this.searchHeaderBtn = page.getByTestId('header-app:Umb.HeaderApp.Search');
    this.searchModal = page.locator('umb-search-modal');
    this.input = this.searchModal.locator('input[name="search-input"]');
    this.providers = this.searchModal.locator('.search-provider');
    this.activeProvider = this.searchModal.locator('.search-provider.active');
    this.results = this.searchModal.locator('.search-item');
    this.activeResult = this.searchModal.locator('.search-item.active');
    this.noResults = this.searchModal.locator('#no-results');
    this.navigationTips = this.searchModal.locator('#navigation-tips');
  }

  async clickSearchHeaderButton() {
    await this.waitForPageLoad();
    await this.click(this.searchHeaderBtn);
    await this.waitForLoadState();
    await this.waitForVisible(this.searchModal, ConstantHelper.timeout.long);
  }

  async clickOutsideToCloseModal() {
    // The modal closes when a click is dispatched anywhere outside of
    // <umb-search-modal>. Coord (0, 0) of the viewport sits in the top-left,
    // outside the centered modal, so the document click handler fires.
    await this.page.mouse.click(0, 0);
  }

  async isSearchModalVisible(isVisible: boolean = true) {
    await this.isVisible(this.searchModal, isVisible, ConstantHelper.timeout.long);
  }

  async enterSearchQuery(query: string) {
    await this.enterText(this.input, query);
  }

  async clearSearchQuery() {
    await this.input.fill('');
  }

  async searchForQuery(query: string, expectedResultName?: string, searchUrl: string = ConstantHelper.apiEndpoints.search) {
    await this.waitForResponseAfterExecutingPromise(searchUrl, this.enterSearchQuery(query), ConstantHelper.statusCodes.ok);

    if (expectedResultName != null) {
      await this.isVisible(this.resultByName(expectedResultName));
    }
  }

  async clickSearchProvider(providerName: string) {
    // Provider labels share prefixes (e.g. "Media" / "Media Types"), so match
    // the button text exactly rather than as a substring.
    await this.click(this.searchModal.getByRole('button', {name: providerName, exact: true}));
  }

  async isSearchProviderActive(providerName: string) {
    await this.hasText(this.activeProvider, providerName);
  }

  async isSearchResultWithNameVisible(name: string, isVisible: boolean = true) {
    await this.isVisible(this.resultByName(name), isVisible);
  }

  async getSearchResultsCount() {
    return await this.results.count();
  }

  async getSearchResultHref(name: string) {
    return await this.resultByName(name).getAttribute('href');
  }

  async isNoResultsMessageVisible(isVisible: boolean = true) {
    await this.isVisible(this.noResults, isVisible);
  }

  async isNavigationTipsVisible(isVisible: boolean = true) {
    await this.isVisible(this.navigationTips, isVisible);
  }

  async pressArrowDown() {
    // Use the page keyboard so we do not refocus the input on subsequent arrow
    // presses (focus moves to the active result link as we navigate).
    await this.page.keyboard.press('ArrowDown');
  }

  async pressArrowUp() {
    await this.page.keyboard.press('ArrowUp');
  }

  async getActiveSearchResultIndex() {
    // Arrow-key handling toggles the .active class synchronously, but we still
    // wait briefly so a stale read (before Lit re-renders) doesn't cause flake.
    await this.hasCount(this.activeResult, 1, ConstantHelper.timeout.short);
    const index = await this.activeResult.getAttribute('data-item-index');
    return index ? parseInt(index, 10) : -1;
  }

  private resultByName(name: string) {
    return this.results.filter({hasText: name});
  }
}
