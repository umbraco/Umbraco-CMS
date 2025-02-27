import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbNumberState } from '@umbraco-cms/backoffice/observable-api';

export class UmbPaginationManager extends EventTarget {
	#defaultValues = {
		totalItems: 0,
		totalPages: 1,
		currentPage: 1,
	};

	#pageSize = new UmbNumberState(10);
	public readonly pageSize = this.#pageSize.asObservable();

	#totalItems = new UmbNumberState(this.#defaultValues.totalItems);
	public readonly totalItems = this.#totalItems.asObservable();

	#totalPages = new UmbNumberState(this.#defaultValues.totalPages);
	public readonly totalPages = this.#totalPages.asObservable();

	#currentPage = new UmbNumberState(this.#defaultValues.currentPage);
	public readonly currentPage = this.#currentPage.asObservable();

	#skip = new UmbNumberState(0);
	public readonly skip = this.#skip.asObservable();

	/**
	 * Sets the number of items per page and recalculates the total number of pages
	 * @param {number} pageSize
	 * @memberof UmbPaginationManager
	 */
	public setPageSize(pageSize: number) {
		this.#pageSize.setValue(pageSize);
		this.#calculateTotalPages();
	}

	/**
	 * Gets the number of items per page
	 * @returns {number}
	 * @memberof UmbPaginationManager
	 */
	public getPageSize() {
		return this.#pageSize.getValue();
	}

	/**
	 * Gets the total number of items
	 * @returns {number}
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
		this.#totalItems.setValue(totalItems);
		this.#calculateTotalPages();
	}

	/**
	 * Gets the total number of pages
	 * @returns {number}
	 * @memberof UmbPaginationManager
	 */
	public getTotalPages() {
		return this.#totalPages.getValue();
	}

	/**
	 * Gets the current page number
	 * @returns {number}
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

		this.#currentPage.setValue(pageNumber);
		this.#calculateSkip();
		this.dispatchEvent(new UmbChangeEvent());
	}

	/**
	 * Gets the number of items to skip
	 * @returns {number}
	 * @memberof UmbPaginationManager
	 */
	public getSkip() {
		return this.#skip.getValue();
	}

	/**
	 * Clears the pagination manager values and resets them to their default values
	 * @memberof UmbPaginationManager
	 */
	public clear() {
		this.#totalItems.setValue(this.#defaultValues.totalItems);
		this.#totalPages.setValue(this.#defaultValues.totalPages);
		this.#currentPage.setValue(this.#defaultValues.currentPage);
		this.#skip.setValue(0);
	}

	/**
	 * Calculates the total number of pages
	 * @memberof UmbPaginationManager
	 */
	#calculateTotalPages() {
		let totalPages = Math.ceil(this.#totalItems.getValue() / this.#pageSize.getValue());
		totalPages = totalPages === 0 ? 1 : totalPages;
		this.#totalPages.setValue(totalPages);

		/* If we currently are on a page higher than the total pages. We need to reset the current page to the last page.
    This can happen if we have a filter that returns less items than the current page size. */
		if (this.getCurrentPageNumber() > totalPages) {
			this.setCurrentPageNumber(totalPages);
		}
	}

	#calculateSkip() {
		const skip = Math.max(0, (this.#currentPage.getValue() - 1) * this.#pageSize.getValue());
		this.#skip.setValue(skip);
	}
}
