import { UmbUserCollectionFilterModel } from '../../types.js';
import { UMB_USER_STORE_CONTEXT_TOKEN, UmbUserStore } from '../../repository/user.store.js';
import { UmbUserCollectionServerDataSource } from './user-collection.server.data.js';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserCollectionRepository implements UmbCollectionRepository {
	#host: UmbControllerHostElement;
	#init;

	#detailStore?: UmbUserStore;
	#collectionSource: UmbCollectionDataSource<UserResponseModel>;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#collectionSource = new UmbUserCollectionServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_USER_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),
		]);
	}

	async requestCollection(filter: UmbUserCollectionFilterModel = { skip: 0, take: 100 }) {
		await this.#init;

		const { data, error } = await this.#collectionSource.filterCollection(filter);

		if (data) {
			this.#detailStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#detailStore!.all() };
	}
}
