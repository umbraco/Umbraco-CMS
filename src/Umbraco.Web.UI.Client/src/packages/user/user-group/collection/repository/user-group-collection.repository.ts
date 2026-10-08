import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_DETAIL_STORE_CONTEXT } from '../../repository/index.js';
import type { UmbUserGroupCollectionFilterModel } from '../types.js';
import { UmbUserGroupCollectionServerDataSource } from './user-group-collection.server.data-source.js';
import type { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { fetchAllPages } from '@umbraco-cms/backoffice/repository';

// Mirrors the server's default page size for `GET /filter/user-group` — chosen so the underlying request matches
// the unconfigured server contract.
const USER_GROUP_PAGE_SIZE = 100;

export class UmbUserGroupCollectionRepository extends UmbControllerBase implements UmbCollectionRepository {
	#init;

	#detailStore?: typeof UMB_USER_GROUP_DETAIL_STORE_CONTEXT.TYPE;
	#collectionSource: UmbCollectionDataSource<UmbUserGroupDetailModel>;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbUserGroupCollectionServerDataSource(this._host);

		this.#init = this.consumeContext(UMB_USER_GROUP_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		})
			.asPromise({ preventTimeout: true })
			// Ignore the error, we can assume that the flow was stopped (asPromise failed), but it does not mean that the consumption was not successful.
			.catch(() => undefined);
	}

	async requestCollection(filter: UmbUserGroupCollectionFilterModel = { skip: 0, take: 100 }) {
		await this.#init;

		if (filter.query) {
			new UmbDeprecation({
				removeInVersion: '19.0.0',
				deprecated: 'User Group requestCollection filter model .query property.',
				solution: 'Use the .filter property instead',
			}).warn();
		}

		const { data, error } = await this.#collectionSource.getCollection(filter);

		if (data) {
			this.#detailStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#detailStore!.all() };
	}

	/**
	 * Requests all user groups by paging through the collection until every item has been retrieved.
	 * Use this in preference to `requestCollection` when callers need the full set — the server defaults
	 * `take` to 100, so a single un-paged request would silently truncate installations with more user groups.
	 * @returns {Promise<{ data?: { items: Array<UmbUserGroupDetailModel>, total: number }, error?: Error, asObservable: () => Observable<Array<UmbUserGroupDetailModel>> }>}
	 * A promise resolving to an object with:
	 * - `data`: `{ items, total }` containing every user group, once all pages have been fetched.
	 * - `error`: set instead of `data` if any page fails.
	 * - `asObservable`: always present; observes every user group currently held in the shared user group detail store,
	 *   which may include groups loaded by other requests.
	 */
	async requestAllItems() {
		await this.#init;

		const { data, error } = await fetchAllPages<UmbUserGroupDetailModel>(
			(skip, take) => this.#collectionSource.getCollection({ skip, take }),
			USER_GROUP_PAGE_SIZE,
		);

		if (data) {
			this.#detailStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#detailStore!.all() };
	}
}

export default UmbUserGroupCollectionRepository;
