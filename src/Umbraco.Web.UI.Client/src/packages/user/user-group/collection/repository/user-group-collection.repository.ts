import { UmbUserGroupCollectionFilterModel } from '../../types.js';
import { UMB_USER_GROUP_STORE_CONTEXT_TOKEN, UmbUserGroupStore } from '../../repository/user-group.store.js';
import { UmbUserGroupCollectionServerDataSource } from './user-group-collection.server.data.js';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserGroupCollectionRepository implements UmbCollectionRepository {
	#host: UmbControllerHostElement;
	#init;

	#detailStore?: UmbUserGroupStore;
	#collectionSource: UmbCollectionDataSource<UserGroupResponseModel>;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#collectionSource = new UmbUserGroupCollectionServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_USER_GROUP_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),
		]);
	}

	async requestCollection(filter: UmbUserGroupCollectionFilterModel = { skip: 0, take: 100 }) {
		await this.#init;

		const { data, error } = await this.#collectionSource.filterCollection(filter);

		if (data) {
			this.#detailStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#detailStore!.all() };
	}
}
