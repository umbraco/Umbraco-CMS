import { UmbExtensionCollectionRepository } from '../collection/repository/extension-collection.repository.js';
import type { UmbExtensionCollectionFilterModel, UmbExtensionCollectionItemModel } from '../collection/types.js';
import { UmbExtensionItemRepository } from '../item/data/item.repository.js';
import type { UmbExtensionPickerConfigCollectionModel } from './types.js';
import type { UmbPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { getConfigValue, type UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export class UmbExtensionPickerDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource<UmbExtensionCollectionItemModel>
{
	#collectionRepository = new UmbExtensionCollectionRepository(this);
	#itemRepository = new UmbExtensionItemRepository(this);
	#config: UmbExtensionPickerConfigCollectionModel | undefined;

	setConfig(config: UmbExtensionPickerConfigCollectionModel | undefined): void {
		this.#config = config;
	}

	getConfig(): UmbConfigCollectionModel | undefined {
		return this.#config;
	}

	async requestCollection(args: UmbCollectionFilterModel) {
		const configAllowedExtensionTypes = getConfigValue(this.#config, 'allowedExtensionTypes');
		const requestedType = (args as UmbExtensionCollectionFilterModel).type;

		let type: string | Array<string> | undefined;

		if (configAllowedExtensionTypes?.length) {
			if (requestedType) {
				// If the UI requests a specific type, only use it if it's in the allowed list
				const requested = Array.isArray(requestedType) ? requestedType : [requestedType];
				type = requested.filter((t) => configAllowedExtensionTypes.includes(t));
			} else {
				// No specific type requested — use all allowed types
				type = configAllowedExtensionTypes;
			}
		}

		const extendedArgs: UmbExtensionCollectionFilterModel = {
			...args,
			type,
		};

		return this.#collectionRepository.requestCollection(extendedArgs);
	}

	async requestItems(uniques: Array<string>) {
		return this.#itemRepository.requestItems(uniques);
	}
}

export { UmbExtensionPickerDataSource as api };
