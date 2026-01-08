import type { UmbElementDetailModel } from '../../types.js';
import { UmbElementServerDataSource } from './element-detail.server.data-source.js';
import { UMB_ELEMENT_DETAIL_STORE_CONTEXT } from './element-detail.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementDetailRepository extends UmbDetailRepositoryBase<
	UmbElementDetailModel,
	UmbElementServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbElementServerDataSource, UMB_ELEMENT_DETAIL_STORE_CONTEXT);
	}

	/**
	 * Gets an existing element by its unique identifier for scaffolding purposes, i.e. to create a new element based on a document type.
	 * @param {string} unique - The unique identifier of the element.
	 * @returns {UmbRepositoryResponse<UmbElementDetailModel>} - The element data.
	 * @memberof UmbElementDetailRepository
	 */
	scaffoldByUnique(unique: string) {
		return this.detailDataSource.scaffoldByUnique(unique);
	}
}

export { UmbElementDetailRepository as api };
