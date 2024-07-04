import type { UmbPartialViewDetailModel } from '../types.js';
import { UmbPartialViewDetailServerDataSource } from './partial-view-detail.server.data-source.js';
import { UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT } from './partial-view-detail.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbPartialViewDetailRepository extends UmbDetailRepositoryBase<UmbPartialViewDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbPartialViewDetailServerDataSource, UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT);
	}
}

export default UmbPartialViewDetailRepository;
