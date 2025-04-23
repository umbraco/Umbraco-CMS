import type { UmbSegmentCollectionFilterModel } from '../types.js';
import { UmbSegmentCollectionServerDataSource } from './segment-collection.server.data-source.js';
import type { UmbSegmentCollectionDataSource } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSegmentCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbSegmentCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbSegmentCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbSegmentCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}
}

export default UmbSegmentCollectionRepository;
