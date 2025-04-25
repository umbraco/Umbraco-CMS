import { UmbMemberRepositoryBase } from '../../repository/member-repository-base.js';
import type { UmbMemberCollectionFilterModel } from '../types.js';
import { UmbMemberCollectionServerDataSource } from './member-collection.server.data-source.js';
import type { UmbMemberCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberCollectionRepository extends UmbMemberRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbMemberCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbMemberCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbMemberCollectionFilterModel = { skip: 0, take: 100 }) {
		await this.init;

		const { data, error } = await this.#collectionSource.getCollection(filter);

		if (data && this.detailStore) {
			this.detailStore.appendItems(data.items);
			return { data, error, asObservable: () => this.detailStore!.all() };
		}

		return { data, error };
	}
}

export default UmbMemberCollectionRepository;
