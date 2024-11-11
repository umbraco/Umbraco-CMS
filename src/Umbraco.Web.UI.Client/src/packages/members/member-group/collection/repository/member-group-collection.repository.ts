import type { UmbMemberGroupCollectionFilterModel } from '../types.js';
import { UmbMemberGroupCollectionServerDataSource } from './member-group-collection.server.data-source.js';
import type { UmbMemberGroupCollectionDataSource } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberGroupCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbMemberGroupCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbMemberGroupCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbMemberGroupCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export default UmbMemberGroupCollectionRepository;
