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
	 * The current configuration of the selection manager.
	 * @returns {UmbCollectionSelectionConfiguration | undefined} - The current configuration of the selection manager.
	 */
	getConfig(): UmbCollectionSelectionConfiguration | undefined {
		return this.#config;
	}
}
