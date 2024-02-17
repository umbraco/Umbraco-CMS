import type { UmbMemberGroupCollectionFilterModel } from '../types.js';
import { UmbMemberGroupCollectionServerDataSource } from './member-group-collection.server.data-source.js';
import type { UmbMemberGroupCollectionDataSource } from './types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberGroupCollectionRepository implements UmbCollectionRepository {
	#collectionSource: UmbMemberGroupCollectionDataSource;

	constructor(host: UmbControllerHost) {
		this.#collectionSource = new UmbMemberGroupCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbMemberGroupCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}

	destroy(): void {}
}

export default UmbMemberGroupCollectionRepository;
