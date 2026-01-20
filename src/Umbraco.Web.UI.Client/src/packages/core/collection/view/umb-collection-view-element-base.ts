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
				this.#collectionContext?.selection.selection,
				(selection) => (this._selection = selection ?? []),
				'umbCollectionSelectionObserver',
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
