import type { UmbStylesheetDetailModel } from '../types.js';
import { UmbStylesheetDetailServerDataSource } from './stylesheet-detail.server.data-source.js';
import { UMB_STYLESHEET_DETAIL_STORE_CONTEXT } from './stylesheet-detail.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStylesheetDetailRepository extends UmbDetailRepositoryBase<UmbStylesheetDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbStylesheetDetailServerDataSource, UMB_STYLESHEET_DETAIL_STORE_CONTEXT);
	}
}

export default UmbStylesheetDetailRepository;
