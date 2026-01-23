import type { UmbCollectionItemModel } from '../types.js';
import type { UmbCollectionSelectionManagerConfig } from './types.js';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

export class UmbCollectionSelectionManager<
	CollectionItemType extends UmbCollectionItemModel = UmbCollectionItemModel,
> extends UmbSelectionManager<UmbCollectionItemModel['unique']> {
	public selectableFilter: (item: CollectionItemType) => boolean = () => true;

	#config?: UmbCollectionSelectionManagerConfig;

	/**
	 * Sets the configuration for the selection manager.
	 * @param {UmbCollectionSelectionManagerConfig | undefined} config - Initial configuration for the selection manager.
	 */
	setConfig(config: UmbCollectionSelectionManagerConfig | undefined) {
		this.#config = config;

		this.setSelectable(this.#config?.selectable ?? false);
		this.setMultiple(this.#config?.multiple ?? false);
		this.setSelection(this.#config?.selection ?? []);

		const selectableFilter = this.#config?.selectableFilter;
		if (selectableFilter) {
			this.selectableFilter = selectableFilter;
		}
	}

	/**
	 * The current configuration of the selection manager.
	 * @returns {UmbCollectionSelectionManagerConfig | undefined} - The current configuration of the selection manager.
	 */
	getConfig(): UmbCollectionSelectionManagerConfig | undefined {
		return this.#config;
	}
}
