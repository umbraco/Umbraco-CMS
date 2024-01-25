import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_DETAIL_STORE_CONTEXT } from './script-detail.store.js';
import { UmbScriptDetailServerDataSource } from './script-detail.server.data-source.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbScriptDetailRepository extends UmbDetailRepositoryBase<UmbScriptDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptDetailServerDataSource, UMB_SCRIPT_DETAIL_STORE_CONTEXT);
	}
}
