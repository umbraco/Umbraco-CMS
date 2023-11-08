import { UmbChangeEvent } from "@umbraco-cms/backoffice/event";
import { UmbNumberState } from "@umbraco-cms/backoffice/observable-api";

export class UmbPaginationManager extends EventTarget {

  #pageSize = new UmbNumberState(10);
  public readonly pageSize = this.#pageSize.asObservable();

  #totalItems = new UmbNumberState(0);
  public readonly totalItems = this.#totalItems.asObservable();

  #totalPages = new UmbNumberState(0);
	public readonly totalPages = this.#totalPages.asObservable();

	#currentPage = new UmbNumberState(1);
	public readonly currentPage = this.#currentPage.asObservable();

  #skip = new UmbNumberState(0);
  public readonly skip = this.#skip.asObservable();

  /**
   * Sets the number of items per page and recalculates the total number of pages
   * @param {number} pageSize
   * @memberof UmbPaginationManager
   */
  public setPageSize(pageSize: number) {
    this.#pageSize.next(pageSize);
    this.#calculateTotalPages();
  }

  /**
   * Gets the number of items per page
   * @return {number} 
   * @memberof UmbPaginationManager
   */
  public getPageSize() {
    return this.#pageSize.getValue();
  }

  /**
   * Gets the total number of items
   * @return {number} 
   * @memberof UmbPaginationManager
   */
  public getTotalItems() {
    return this.#totalItems.getValue();
  }

  /**
   * Sets the total number of items and recalculates the total number of pages
   * @param {number} totalItems
   * @memberof UmbPaginationManager
   */
  public setTotalItems(totalItems: number) {
    this.#totalItems.next(totalItems);
    this.#calculateTotalPages();
  }

  /**
   * Gets the total number of pages
   * @return {number} 
   * @memberof UmbPaginationManager
   */
  public getTotalPages() {
    return this.#totalPages.getValue();
  }

  /**
   * Gets the current page number
   * @return {number} 
   * @memberof UmbPaginationManager
   */
  public getCurrentPageNumber() {
    return this.#currentPage.getValue();
  }

  /**
   * Sets the current page number
   * @param {number} pageNumber
   * @memberof UmbPaginationManager
   */
  public setCurrentPageNumber(pageNumber: number) {
    if (pageNumber < 1) {
      pageNumber = 1;
    }

    if (pageNumber > this.#totalPages.getValue()) {
      pageNumber = this.#totalPages.getValue();
    }

    this.#currentPage.next(pageNumber);
    this.#calculateSkip();
    this.dispatchEvent(new UmbChangeEvent());
	}

  /**
   * Gets the number of items to skip
   * @return {number} 
   * @memberof UmbPaginationManager
   */
  public getSkip() {
    return this.#skip.getValue();
  }

  /**
   * Calculates the total number of pages
   * @memberof UmbPaginationManager
   */
  #calculateTotalPages() {
    const totalPages = Math.ceil(this.#totalItems.getValue() / this.#pageSize.getValue());
    this.#totalPages.next(totalPages);

    /* If we currently are on a page higher than the total pages. We need to reset the current page to the last page.
    This can happen if we have a filter that returns less items than the current page size. */
    if (this.getCurrentPageNumber() > totalPages) {
      this.setCurrentPageNumber(totalPages);
    }
  }
  
  #calculateSkip() {
    const skip = (this.#currentPage.getValue() - 1) * this.#pageSize.getValue();
    this.#skip.next(skip);
  }
}