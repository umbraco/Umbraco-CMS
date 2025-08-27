import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbArrayState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbTargetPaginationManager<
	ItemModelType extends UmbEntityModel = UmbEntityModel,
> extends UmbControllerBase {
	#currentItems = new UmbArrayState<ItemModelType>([], (x) => x.unique);
	#baseTarget = new UmbObjectState<ItemModelType | undefined>(undefined);

	#pageSize = new UmbNumberState(10);
	public readonly pageSize = this.#pageSize.asObservable();

	#totalItems = new UmbNumberState(0);
	public readonly totalItems = this.#totalItems.asObservable();

	#totalPrevItems = new UmbNumberState(0);
	public readonly totalPrevItems = this.#totalPrevItems.asObservable();

	#totalNextItems = new UmbNumberState(0);
	public readonly totalNextItems = this.#totalNextItems.asObservable();

	/**
	 * Gets the target that the pagination is based around
	 * @returns {ItemModelType | undefined} - The target item that the pagination is based around
	 * @memberof UmbTargetPaginationManager
	 */
	getBaseTarget(): ItemModelType | undefined {
		return this.#baseTarget.getValue();
	}

	/**
	 * Set the target that the pagination will be based around
	 * @param {(ItemModelType | undefined)} target - The target
	 * @memberof UmbTargetPaginationManager
	 */
	setBaseTarget(target: ItemModelType | undefined) {
		this.#baseTarget.setValue(target);
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
	 * Sets the number of items per page and recalculates the total number of pages
	 * @param {number} pageSize
	 * @memberof UmbPaginationManager
	 */
	public setPageSize(pageSize: number) {
		this.#pageSize.setValue(pageSize);
	}

	/**
	 * Gets the items currently used in the pagination calculations.
	 * @returns {Array<ItemModelType>} - The current items that are part of the paging
	 * @memberof UmbTargetPaginationManager
	 */
	public getCurrentItems(): Array<ItemModelType> {
		return this.#currentItems.getValue();
	}

	/**
	 * Sets the items that will be used in the pagination manager. The items will be used to calculate start and end targets.
	 * @param {Array<ItemModelType>} items - The items to set
	 * @memberof UmbTargetPaginationManager
	 */
	public setCurrentItems(items: Array<ItemModelType>) {
		this.#currentItems.setValue(items);
	}

	/**
	 * Prepend more items to the manager. Use when more items before the base target has been loaded.
	 * @param {Array<ItemModelType>} items - The items to prepend
	 * @memberof UmbTargetPaginationManager
	 */
	public prependCurrentItems(items: Array<ItemModelType>) {
		this.#currentItems.prepend(items);
	}

	/**
	 * Append more items to the manager. Use when more item after the base target has been loaded.
	 * @param {Array<ItemModelType>} items - The items to append
	 * @returns {void}
	 * @memberof UmbTargetPaginationManager
	 */
	public appendCurrentItems(items: Array<ItemModelType>): void {
		this.#currentItems.append(items);
	}

	/**
	 * Gets the total number of items
	 * @returns {number}
	 * @memberof UmbPaginationManager
	 */
	public getTotalItems(): number {
		return this.#totalItems.getValue();
	}

	/**
	 * Sets the total number of items and recalculates the total number of pages
	 * @param {number} totalItems
	 * @memberof UmbPaginationManager
	 */
	public setTotalItems(totalItems: number) {
		this.#totalItems.setValue(totalItems);
	}

	/**
	 * Gets the next target that should be used to load items before the base target
	 * @returns {ItemModelType | undefined} - The target item to load more items before
	 * @memberof UmbTargetPaginationManager
	 */
	public getNextStartTarget(): ItemModelType | undefined {
		return this.#currentItems.getValue()[0];
	}

	/**
	 *
	 * @returns {ItemModelType | undefined} - The target item to load more items before
	 * @memberof UmbTargetPaginationManager
	 */
	public getNextEndTarget(): ItemModelType | undefined {
		return this.#currentItems.getValue().slice(-1)[0];
	}

	public setTotalItemsBeforeStartTarget(totalItems: number | undefined) {
		this.#totalPrevItems.setValue(totalItems ?? 0);
	}

	public setTotalItemsAfterEndTarget(totalItems: number | undefined): void {
		const calculatedTotalItems =
			totalItems !== undefined ? totalItems : this.getTotalItems() - this.#currentItems.getValue().length;
		this.#totalNextItems.setValue(calculatedTotalItems);
	}

	/**
	 * Clears the pagination manager values and resets them to their default values
	 * @memberof UmbPaginationManager
	 */
	public clear() {
		this.#totalItems.setValue(0);
	}
}
