import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_DETAIL_STORE_CONTEXT } from './script-detail.store.context-token.js';
import { UmbScriptDetailServerDataSource } from './script-detail.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbScriptDetailRepository extends UmbDetailRepositoryBase<UmbScriptDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptDetailServerDataSource, UMB_SCRIPT_DETAIL_STORE_CONTEXT);
	}
}

export default UmbScriptDetailRepository;
