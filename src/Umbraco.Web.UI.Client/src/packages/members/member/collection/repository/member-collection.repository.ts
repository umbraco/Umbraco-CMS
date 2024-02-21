import type { UmbMemberCollectionFilterModel } from '../types.js';
import { UmbMemberCollectionServerDataSource } from './member-collection.server.data-source.js';
import type { UmbMemberCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberCollectionRepository implements UmbCollectionRepository {
	#collectionSource: UmbMemberCollectionDataSource;

	constructor(host: UmbControllerHost) {
		this.#collectionSource = new UmbMemberCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbMemberCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}

	destroy(): void {}
}

export default UmbMemberCollectionRepository;
