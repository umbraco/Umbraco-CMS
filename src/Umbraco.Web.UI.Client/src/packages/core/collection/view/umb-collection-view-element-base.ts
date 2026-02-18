import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { UmbCollectionItemModel } from '../types.js';
import { state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Abstract base class for collection view elements.
 * Provides shared state management, selection handling, and context consumption for collection views.
 */
export abstract class UmbCollectionViewElementBase extends UmbLitElement {
	@state()
	protected _items: Array<UmbCollectionItemModel> = [];

	@state()
	protected _selectable = false;

	@state()
	protected _multiple = false;

	@state()
	protected _selectOnly = false;

	@state()
	protected _selection: Array<string | null> = [];

	@state()
	protected _loading = false;

	@state()
	protected _itemHrefs: Map<string, string> = new Map();

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;

			this.observe(
				this.#collectionContext?.selection.selectable,
				(selectable) => (this._selectable = selectable ?? false),
				'umbCollectionSelectableObserver',
			);

			this.observe(
				this.#collectionContext?.selectOnly,
				(selectOnly) => {
					this._selectOnly = selectOnly ?? false;
				},
				'umbCollectionSelectOnlyObserver',
			);

			this.observe(
				this.#collectionContext?.selection.selection,
				(selection) => (this._selection = selection ?? []),
				'umbCollectionSelectionObserver',
			);

			this.observe(
				this.#collectionContext?.selection.multiple,
				(multiple) => (this._multiple = multiple ?? false),
				'umbCollectionSelectionMultipleObserver',
			);

			this.observe(
				this.#collectionContext?.items,
				async (items) => {
					this._items = items ?? [];
					await this.#updateItemHrefs();
				},
				'umbCollectionItemsObserver',
			);
		});
	}

	/**
	 * Determines if an item is selectable
	 * @param {UmbCollectionItemModel} item - The item to check for selectability.
	 * @returns {boolean} True if the item is selectable, false otherwise.
	 */
	protected _isSelectableItem(item: UmbCollectionItemModel): boolean {
		if (!this._selectable) {
			return false;
		}

		if (!this.#collectionContext?.selection.selectableFilter) {
			return true;
		}

		return this.#collectionContext.selection.selectableFilter(item);
	}

	/**
	 * Selects an item in the collection.
	 * @param {string} unique - The unique identifier of the item to select.
	 */
	protected _selectItem(unique: UmbCollectionItemModel['unique']) {
		this.#collectionContext?.selection.select(unique);
	}

	/**
	 * Deselects an item in the collection.
	 * @param {string} unique - The unique identifier of the item to deselect.
	 */
	protected _deselectItem(unique: UmbCollectionItemModel['unique']) {
		this.#collectionContext?.selection.deselect(unique);
	}

	/**
	 * Sets the current selection in the collection.
	 * @param {Array<string>} selection - An array of unique identifiers representing the new selection.
	 */
	protected _setSelection(selection: Array<string>) {
		this.#collectionContext?.selection.setSelection(selection);
	}

	/**
	 * Checks if an item is currently selected.
	 * @param {string} unique - The unique identifier of the item to check.
	 * @returns {boolean} True if the item is selected, false otherwise.
	 */
	protected _isSelectedItem(unique: UmbCollectionItemModel['unique']): boolean {
		return this.#collectionContext?.selection.isSelected(unique) ?? false;
	}

	async #updateItemHrefs() {
		const entries = await Promise.all(
			this._items.map(async (item) => {
				const href = await this.#collectionContext?.requestItemHref?.(item);
				return item.unique && href ? ([item.unique, href] as const) : null;
			}),
		);
		this._itemHrefs = new Map(entries.filter((entry): entry is [string, string] => entry !== null));
	}
}
