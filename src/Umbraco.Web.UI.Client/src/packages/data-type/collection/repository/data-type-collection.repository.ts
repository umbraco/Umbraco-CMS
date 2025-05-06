import { UMB_DATA_TYPE_ITEM_STORE_CONTEXT } from '../../constants.js';
import type { UmbDataTypeItemStore } from '../../repository/item/data-type-item.store.js';
import type { UmbDataTypeCollectionFilterModel } from '../types.js';
import { UmbDataTypeCollectionServerDataSource } from './data-type-collection.server.data-source.js';
import type { UmbDataTypeCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDataTypeCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#init;
	#itemStore?: UmbDataTypeItemStore;
	#collectionSource: UmbDataTypeCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = this.consumeContext(UMB_DATA_TYPE_ITEM_STORE_CONTEXT, (instance) => {
			if (instance) {
				this.#itemStore = instance;
			}
		}).asPromise({ preventTimeout: true });

		this.#collectionSource = new UmbDataTypeCollectionServerDataSource(host);
	}

	async requestCollection(query: UmbDataTypeCollectionFilterModel) {
		await this.#init;

		const { data, error } = await this.#collectionSource.getCollection(query);

		if (data) {
			this.#itemStore!.appendItems(data.items);
		}

		const uniques = data?.items.map((item) => item.unique) ?? [];

		return { data, error, asObservable: () => this.#itemStore!.items(uniques) };
	}
}

export default UmbDataTypeCollectionRepository;
