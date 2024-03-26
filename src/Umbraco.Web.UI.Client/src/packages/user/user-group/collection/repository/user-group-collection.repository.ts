import type { UmbUserGroupDetailModel } from '../../types.js';
import type { UmbUserGroupDetailStore } from '../../repository/index.js';
import { UMB_USER_GROUP_DETAIL_STORE_CONTEXT } from '../../repository/index.js';
import type { UmbUserGroupCollectionFilterModel } from '../types.js';
import { UmbUserGroupCollectionServerDataSource } from './user-group-collection.server.data-source.js';
import type { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbUserGroupCollectionRepository extends UmbControllerBase implements UmbCollectionRepository {
	#init;

	#detailStore?: UmbUserGroupDetailStore;
	#collectionSource: UmbCollectionDataSource<UmbUserGroupDetailModel>;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbUserGroupCollectionServerDataSource(this._host);

		this.#init = this.consumeContext(UMB_USER_GROUP_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		}).asPromise();
	}

	async requestCollection(filter: UmbUserGroupCollectionFilterModel = { skip: 0, take: 100 }) {
		await this.#init;

		const { data, error } = await this.#collectionSource.getCollection(filter);

		if (data) {
			this.#detailStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#detailStore!.all() };
	}
}

export default UmbUserGroupCollectionRepository;
