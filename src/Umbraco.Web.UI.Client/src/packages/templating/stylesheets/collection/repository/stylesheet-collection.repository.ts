import { UmbStylesheetCollectionFilterModel, UmbStylesheetCollectionItemModel } from '../types.js';
import { UmbStylesheetDetailStore, UMB_STYLESHEET_DETAIL_STORE_CONTEXT } from '../../repository/index.js';
import { UmbStylesheetCollectionServerDataSource } from './stylesheet-collection.server.data-source.js';
import { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';

export class UmbStylesheetCollectionRepository extends UmbBaseController implements UmbCollectionRepository {
	#init;

	#detailStore?: UmbStylesheetDetailStore;
	#collectionSource: UmbCollectionDataSource<UmbStylesheetCollectionItemModel>;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbStylesheetCollectionServerDataSource(this._host);

		this.#init = this.consumeContext(UMB_STYLESHEET_DETAIL_STORE_CONTEXT, (instance) => {
			this.#detailStore = instance;
		}).asPromise();
	}

	async requestCollection(filter: UmbStylesheetCollectionFilterModel = { take: 100, skip: 0 }) {
		await this.#init;

		const { data, error } = await this.#collectionSource.getCollection(filter);

		if (data) {
			this.#detailStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#detailStore!.all() };
	}
}
