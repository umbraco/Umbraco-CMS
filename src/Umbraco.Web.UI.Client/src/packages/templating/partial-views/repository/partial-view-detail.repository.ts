import { UmbPartialViewDetailModel } from '../types.js';
import { UmbPartialViewDetailServerDataSource } from './partial-view-detail.server.data-source.js';
import { UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT } from './partial-view-detail.store.js';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbPartialViewDetailRepository extends UmbDetailRepositoryBase<UmbPartialViewDetailModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UmbPartialViewDetailServerDataSource, UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT);
	}
}
