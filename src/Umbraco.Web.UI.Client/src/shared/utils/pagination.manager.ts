import { UmbNumberState } from "@umbraco-cms/backoffice/observable-api";

export class UmbPaginationManager {

  #pageSize = new UmbNumberState(10);
  public readonly pageSize = this.#pageSize.asObservable();

  #totalItems = new UmbNumberState(0);
  public readonly totalItems = this.#totalItems.asObservable();

  #totalPages = new UmbNumberState(0);
	public readonly totalPages = this.#totalPages.asObservable();

	#currentPage = new UmbNumberState(1);
	public readonly currentPage = this.#currentPage.asObservable();

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
    this.#currentPage.next(pageNumber);
	}

  /**
   * Calculates the total number of pages
   * @memberof UmbPaginationManager
   */
  #calculateTotalPages() {
    const totalPages = Math.ceil(this.#totalItems.getValue() / this.#pageSize.getValue());
    this.#totalPages.next(totalPages);
  }
}