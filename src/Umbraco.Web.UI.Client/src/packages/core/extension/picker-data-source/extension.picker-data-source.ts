import { UmbExtensionCollectionRepository } from '../collection/data/collection.repository.js';
import type { UmbExtensionCollectionFilterModel, UmbExtensionCollectionItemModel } from '../collection/types.js';
import { UmbExtensionItemRepository } from '../item/data/item.repository.js';
import type { UmbPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export class UmbExtensionPickerDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource<UmbExtensionCollectionItemModel>
{
	#collectionRepository = new UmbExtensionCollectionRepository(this);
	#itemRepository = new UmbExtensionItemRepository(this);
	#config: UmbConfigCollectionModel | undefined;

	setConfig(config: UmbConfigCollectionModel | undefined): void {
		this.#config = config;
	}

	getConfig(): UmbConfigCollectionModel | undefined {
		return this.#config;
	}

	async requestCollection(filter: UmbCollectionFilterModel) {
		const allowedExtensionTypes = this.#config?.find((c) => c.alias === 'allowedExtensionTypes')?.value as
			| Array<string>
			| undefined;

		const extendedFilter: UmbExtensionCollectionFilterModel = {
			...filter,
			extensionTypes: allowedExtensionTypes,
		};

		return this.#collectionRepository.requestCollection(extendedFilter);
	}

	async requestItems(uniques: Array<string>) {
		return this.#itemRepository.requestItems(uniques);
	}
}

export { UmbExtensionPickerDataSource as api };
