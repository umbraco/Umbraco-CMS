import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbTargetPaginationManager<
	ItemModelType extends UmbEntityModel = UmbEntityModel,
> extends UmbControllerBase {
	#baseTarget = new UmbObjectState<UmbEntityModel | undefined>(undefined);
	#currentItems = new UmbArrayState<ItemModelType>([], (x) => x.unique);

	#takeSize = new UmbNumberState(50);
	public readonly takeSize = this.#takeSize.asObservable();

	#totalItems = new UmbNumberState(0);
	public readonly totalItems = this.#totalItems.asObservable();

	#totalPrevItems = new UmbNumberState(0);
	public readonly totalPrevItems = this.#totalPrevItems.asObservable();

	#totalNextItems = new UmbNumberState(0);
	public readonly totalNextItems = this.#totalNextItems.asObservable();

	/**
	 * Gets the target that the pagination is based around
	 * @returns {UmbEntityModel | undefined} - The target item that the pagination is based around
	 * @memberof UmbTargetPaginationManager
	 */
	public getBaseTarget(): UmbEntityModel | undefined {
		return this.#baseTarget.getValue();
	}

	/**
	 * Set the target that the pagination will be based around
	 * @param {(UmbEntityModel | undefined)} target - The target
	 * @memberof UmbTargetPaginationManager
	 */
	public setBaseTarget(target: UmbEntityModel | undefined) {
		this.#baseTarget.setValue(target);
	}

	/**
	 * Gets a new baseTarget from the already loaded items
	 * @returns {UmbEntityModel | undefined} - The new target item or undefined if no new target could be found
	 * @memberof UmbTargetPaginationManager
	 */
	public getNewBaseTarget(): UmbEntityModel | undefined {
		const currentTarget = this.getBaseTarget();
		if (!currentTarget) {
			return undefined;
		}

		const currentItems = this.#currentItems.getValue();
		const currentBaseTargetIndex = currentItems.findIndex((item) => item.unique === currentTarget.unique);
		if (currentBaseTargetIndex === -1) {
			return undefined;
		}

		/* When choosing a new base target we check on both sides of the current base target
		as the base target could be the first or last item */
		const nextItem = currentItems[currentBaseTargetIndex + 1];
		const previousItem = currentItems[currentBaseTargetIndex - 1];

		return nextItem || previousItem;
	}

	/**
	 * Checks if the Base Target is part of the current items
	 * @returns {boolean} True if the Base Target is in the current items, false otherwise
	 * @memberof UmbTargetPaginationManager
	 */
	public hasBaseTargetInCurrentItems(): boolean {
		try {
			return this.#getIndexOfBaseTarget() !== -1;
		} catch {
			return false;
		}
	}

	/**
	 * Gets the number of items per page
	 * @returns {number}
	 * @memberof UmbPaginationManager
	 */
	public getTakeSize(): number {
		return this.#takeSize.getValue();
	}

	/**
	 * Sets the number of items per page and recalculates the total number of pages
	 * @param {number} pageSize
	 * @memberof UmbPaginationManager
	 */
	public setTakeSize(pageSize: number) {
		this.#takeSize.setValue(pageSize);
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
	 * Removes an item from the current items
	 * @param {UmbEntityModel} entity
	 * @memberof UmbTargetPaginationManager
	 */
	public removeFromCurrentItems(entity: UmbEntityModel) {
		this.#currentItems.remove([entity.unique]);
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
	 * Gets the first target of the current item. Use for loading more items before the base target
	 * @returns {UmbEntityModel} - The target item to load more items before
	 * @memberof UmbTargetPaginationManager
	 */
	public getStartTarget(): UmbEntityModel {
		const firstItem = this.#currentItems.getValue()[0];

		if (!firstItem) {
			throw new Error('No Start Target found');
		}

		return { entityType: firstItem.entityType, unique: firstItem.unique };
	}

	/**
	 * Gets a new start target for the current item. Use for loading more items before the base target
	 * @returns {UmbEntityModel | undefined} - The target item to load more items before
	 * @memberof UmbTargetPaginationManager
	 */
	public getNewStartTarget(): UmbEntityModel | undefined {
		const currentTarget = this.getStartTarget();
		if (!currentTarget) {
			return undefined;
		}

		const currentItems = this.#currentItems.getValue();
		const currentBaseTargetIndex = currentItems.findIndex((item) => item.unique === currentTarget.unique);
		if (currentBaseTargetIndex === -1) {
			return undefined;
		}

		// When choosing a new start target, we want the next item in the list
		return currentItems[currentBaseTargetIndex + 1];
	}

	/**
	 * Gets the last target of the current items. Use for load more items after the base target
	 * @returns {UmbEntityModel} - The target item to load more items before
	 * @memberof UmbTargetPaginationManager
	 */
	public getEndTarget(): UmbEntityModel {
		const lastItem = this.#currentItems.getValue().slice(-1)[0];

		if (!lastItem) {
			throw new Error('No End Target found');
		}

		return { entityType: lastItem.entityType, unique: lastItem.unique };
	}

	/**
	 * Gets a new end target for the current item. Use for loading more items after the base target
	 * @returns {UmbEntityModel | undefined} - The target item to load more items after
	 * @memberof UmbTargetPaginationManager
	 */
	public getNewEndTarget(): UmbEntityModel | undefined {
		const currentTarget = this.getEndTarget();
		if (!currentTarget) {
			return undefined;
		}

		const currentItems = this.#currentItems.getValue();
		const currentBaseTargetIndex = currentItems.findIndex((item) => item.unique === currentTarget.unique);
		if (currentBaseTargetIndex === -1) {
			return undefined;
		}

		// When choosing a new end target, we want the previous item in the list
		return currentItems[currentBaseTargetIndex - 1];
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
	 * Gets the number of current items before the target
	 * @returns {number} - The number of items
	 * @memberof UmbTargetPaginationManager
	 */
	public getNumberOfCurrentItemsBeforeBaseTarget(): number {
		return this.#getIndexOfBaseTarget();
	}

	/**
	 * Gets the number of current items after the target
	 * @returns {number} - The number of items
	 * @memberof UmbTargetPaginationManager
	 */
	public getNumberOfCurrentItemsAfterBaseTarget(): number {
		// find the total number of items after the base target
		const baseTargetIndex = this.#getIndexOfBaseTarget();
		return this.#currentItems.getValue().length - baseTargetIndex - 1;
	}

	/**
	 * Clears the pagination manager values and resets them to their default values
	 * @memberof UmbPaginationManager
	 */
	public clear() {
		this.#totalItems.setValue(0);
		this.#currentItems.clear();
		this.#totalPrevItems.setValue(0);
		this.#totalNextItems.setValue(0);
		this.#baseTarget.setValue(undefined);
	}

	#getIndexOfBaseTarget() {
		const baseTarget = this.getBaseTarget();

		if (!baseTarget) {
			throw new Error('Base target is not set');
		}

		const currentItems = this.#currentItems.getValue();
		const index = currentItems.findIndex((item) => item.unique === baseTarget.unique);

		if (index === -1) {
			throw new Error('Base target is not in the current items');
		}

		return index;
	}
}
