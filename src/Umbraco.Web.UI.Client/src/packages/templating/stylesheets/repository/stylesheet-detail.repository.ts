import { UmbStylesheetDetailModel } from '../types.js';
import { UmbStylesheetDetailServerDataSource } from './stylesheet-detail.server.data-source.js';
import { UMB_STYLESHEET_DETAIL_STORE_CONTEXT } from './stylesheet-detail.store.js';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbStylesheetDetailRepository extends UmbDetailRepositoryBase<UmbStylesheetDetailModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UmbStylesheetDetailServerDataSource, UMB_STYLESHEET_DETAIL_STORE_CONTEXT);
	}
}
