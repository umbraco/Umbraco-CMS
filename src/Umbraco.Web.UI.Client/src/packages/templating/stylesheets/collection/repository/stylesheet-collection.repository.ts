import { UmbStylesheetCollectionFilterModel, UmbStylesheetCollectionItemModel } from '../types.js';
import { UmbStylesheetCollectionServerDataSource } from './stylesheet-collection.server.data-source.js';
import { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';

export class UmbStylesheetCollectionRepository extends UmbBaseController implements UmbCollectionRepository {
	#collectionSource: UmbCollectionDataSource<UmbStylesheetCollectionItemModel>;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionSource = new UmbStylesheetCollectionServerDataSource(this._host);
	}

	/**
	 * Requests the stylesheet collection
	 * @param {UmbStylesheetCollectionFilterModel} [filter={ take: 100, skip: 0 }]
	 * @return {*}
	 * @memberof UmbStylesheetCollectionRepository
	 */
	requestCollection(filter: UmbStylesheetCollectionFilterModel = { take: 100, skip: 0 }) {
		return this.#collectionSource.getCollection(filter);
	}
}
