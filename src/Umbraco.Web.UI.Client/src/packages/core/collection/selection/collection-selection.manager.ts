import type { UmbCollectionItemModel, UmbCollectionSelectionConfiguration } from '../types.js';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

export class UmbCollectionSelectionManager<
	CollectionItemType extends UmbCollectionItemModel = UmbCollectionItemModel,
> extends UmbSelectionManager<UmbCollectionItemModel['unique']> {
	public selectableFilter: (item: CollectionItemType) => boolean = () => true;

	#config?: UmbCollectionSelectionConfiguration;

	/**
	 * Sets the configuration for the selection manager.
	 * @param {UmbCollectionSelectionConfiguration | undefined} config - Initial configuration for the selection manager.
	 */
	setConfig(config: UmbCollectionSelectionConfiguration | undefined) {
		this.#config = config;

		this.setSelectable(this.#config?.selectable ?? false);
		this.setMultiple(this.#config?.multiple ?? false);
		this.setSelectOnly(this.#config?.selectOnly ?? false);
		this.setSelection(this.#config?.selection ?? []);

		const selectableFilter = this.#config?.selectableFilter;
		if (selectableFilter) {
			this.selectableFilter = selectableFilter;
		}
	}

	/**
	 * Selects an item by its unique identifier.
	 * Enables select-only mode based on the configuration before selecting.
	 * @param {string} unique - The unique identifier of the item to select.
	 */
	override select(unique: string): void {
		/* In collection we want to enable select-only mode when selecting an item so we don't accidentally navigate away. */
		this.setSelectOnly(true);
		super.select(unique);
	}

	/**
	 * Deselects an item by its unique identifier.
	 * Disables select-only mode based on the configuration before deselecting.
	 * @param {string} unique - The unique identifier of the item to deselect.
	 */
	override deselect(unique: string): void {
		// In collection we want to disable select-only mode when deselecting an item to allow navigation.
		// This only applies if the config doesn't enforce select-only.
		this.setSelectOnly(this.#config?.selectOnly ?? false);
		super.deselect(unique);
	}

	/**
	 * Clears all selected items.
	 * Disables select-only mode based on the configuration before clearing.
	 */
	override clearSelection(): void {
		// In collection we want to disable select-only mode when clearing selection to allow navigation.
		// This only applies if the config doesn't enforce select-only.
		this.setSelectOnly(this.#config?.selectOnly ?? false);
		super.clearSelection();
	}

	/**
	 * The current configuration of the selection manager.
	 * @returns {UmbCollectionSelectionConfiguration | undefined} - The current configuration of the selection manager.
	 */
	getConfig(): UmbCollectionSelectionConfiguration | undefined {
		return this.#config;
	}
}
