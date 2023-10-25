import { UmbUserCollectionFilterModel } from '../../types.js';
import { UmbUserRepositoryBase } from '../../repository/user-repository-base.js';
import { UmbUserCollectionServerDataSource } from './user-collection.server.data.js';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserCollectionRepository extends UmbUserRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbCollectionDataSource<UserResponseModel>;

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.#collectionSource = new UmbUserCollectionServerDataSource(this.host);
	}

	async requestCollection(filter: UmbUserCollectionFilterModel = { skip: 0, take: 100 }) {
		await this.init;

		const { data, error } = await this.#collectionSource.filterCollection(filter);

		if (data) {
			this.detailStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.detailStore!.all() };
	}
}
