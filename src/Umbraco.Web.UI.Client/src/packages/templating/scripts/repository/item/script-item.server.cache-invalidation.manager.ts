import { scriptItemCache } from './script-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ScriptItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiScriptItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<ScriptItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: scriptItemCache,
			eventSources: ['Umbraco:CMS:Script'],
		});
	}
}
