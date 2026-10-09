import type { UmbLanguageCollectionFilterModel } from '../types.js';
import type { UmbLanguageDetailModel } from '../../types.js';
import { UmbLanguageCollectionServerDataSource } from './language-collection.server.data-source.js';
import type { UmbLanguageCollectionDataSource } from './types.js';
import { UmbRepositoryBase, fetchAllPages } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// Mirrors the server's default page size for `GET /language` — chosen so the underlying request matches
// the unconfigured server contract.
const LANGUAGE_PAGE_SIZE = 100;

export class UmbLanguageCollectionRepository extends UmbRepositoryBase implements UmbCollectionRepository {
	#collectionSource: UmbLanguageCollectionDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbLanguageCollectionServerDataSource(host);
	}

	async requestCollection(filter: UmbLanguageCollectionFilterModel) {
		return this.#collectionSource.getCollection(filter);
	}

	/**
	 * Requests all languages by paging through the collection until every item has been retrieved.
	 * Use this in preference to `requestCollection` when callers need the full set — the server defaults
	 * `take` to 100, so a single un-paged request would silently truncate installations with more languages.
	 * @returns {Promise} A promise resolving to `{ data: { items, total } }` containing every language, or `{ error }`.
	 */
	async requestAllItems() {
		return fetchAllPages<UmbLanguageDetailModel>(
			(skip, take) => this.#collectionSource.getCollection({ skip, take }),
			LANGUAGE_PAGE_SIZE,
		);
	}
}

export default UmbLanguageCollectionRepository;
