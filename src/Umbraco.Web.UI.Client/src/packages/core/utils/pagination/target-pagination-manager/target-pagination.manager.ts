import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState, UmbNumberState } from '@umbraco-cms/backoffice/observable-api';

export class UmbTargetPaginationManager extends UmbControllerBase {
	#defaultValues = {
		take: 10,
		totalItems: 0,
	};

	#pageSize = new UmbNumberState(this.#defaultValues.take);
	public readonly pageSize = this.#pageSize.asObservable();

	#totalItems = new UmbNumberState(this.#defaultValues.totalItems);
	public readonly totalItems = this.#totalItems.asObservable();

	#hasMoreItems = new UmbBooleanState(false);
	public readonly hasMoreItems = this.#hasMoreItems.asObservable();

	/**
	 * Sets the number of items per page and recalculates the total number of pages
	 * @param {number} pageSize
	 * @memberof UmbPaginationManager
	 */
	public setPageSize(pageSize: number) {
		this.#pageSize.setValue(pageSize);
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
		debugger;
		this.#hasMoreItems.setValue(totalItems > 0);
	}

	/**
	 * Clears the pagination manager values and resets them to their default values
	 * @memberof UmbPaginationManager
	 */
	public clear() {
		this.#totalItems.setValue(this.#defaultValues.totalItems);
	}
}
