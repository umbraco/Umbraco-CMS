import { UmbUserRepositoryBase } from '../../repository/user-repository-base.js';
import type { UmbUserCollectionFilterModel } from '../types.js';
import { UmbUserCollectionServerDataSource } from './user-collection.server.data-source.js';
import type { UmbUserCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserCollectionRepository extends UmbUserRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbUserCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbUserCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbUserCollectionFilterModel = { skip: 0, take: 100 }) {
		await this.init;

		const { data, error } = await this.#collectionSource.getCollection(filter);

		if (data) {
			this.detailStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.detailStore!.all() };
	}
}

export default UmbUserCollectionRepository;
