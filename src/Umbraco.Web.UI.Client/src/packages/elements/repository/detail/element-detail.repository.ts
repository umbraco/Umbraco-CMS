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
}

export { UmbElementDetailRepository as api };
