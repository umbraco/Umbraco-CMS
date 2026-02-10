import { UmbExtensionCollectionRepository } from '../collection/repository/extension-collection.repository.js';
import type { UmbExtensionCollectionFilterModel, UmbExtensionCollectionItemModel } from '../collection/types.js';
import { UmbExtensionItemRepository } from '../item/data/item.repository.js';
import type { UmbExtensionPickerDataSourceConfigCollectionModel } from './types.js';
import type { UmbPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { getConfigValue, type UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export class UmbExtensionPickerDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource<UmbExtensionCollectionItemModel>
{
	#collectionRepository = new UmbExtensionCollectionRepository(this);
	#itemRepository = new UmbExtensionItemRepository(this);
	#config: UmbExtensionPickerDataSourceConfigCollectionModel | undefined;

	setConfig(config: UmbExtensionPickerDataSourceConfigCollectionModel | undefined): void {
		this.#config = config;
	}

	getConfig(): UmbConfigCollectionModel | undefined {
		return this.#config;
	}

	async requestCollection(args: UmbExtensionCollectionFilterModel) {
		const configAllowedExtensionTypes = getConfigValue(this.#config, 'allowedExtensionTypes');
		const requestedTypes = args.extensionTypes;

		let extensionTypes: Array<string> | undefined;

		if (configAllowedExtensionTypes?.length) {
			if (requestedTypes?.length) {
				// If the UI requests specific types, only use those that are in the allowed list
				extensionTypes = requestedTypes.filter((t) => configAllowedExtensionTypes.includes(t));
			} else {
				// No specific types requested — use all allowed types
				extensionTypes = configAllowedExtensionTypes;
			}
		} else {
			// No config restriction — pass through whatever the caller requested
			extensionTypes = requestedTypes;
		}

		const extendedArgs: UmbExtensionCollectionFilterModel = {
			...args,
			extensionTypes,
		};

		return this.#collectionRepository.requestCollection(extendedArgs);
	}

	async requestItems(uniques: Array<string>) {
		return this.#itemRepository.requestItems(uniques);
	}
}

export { UmbExtensionPickerDataSource as api };
