import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_DETAIL_STORE_CONTEXT } from '../../repository/index.js';
import type { UmbUserGroupCollectionFilterModel } from '../types.js';
import { UmbUserGroupCollectionServerDataSource } from './user-group-collection.server.data-source.js';
import type { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

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
}

export default UmbUserGroupCollectionRepository;
