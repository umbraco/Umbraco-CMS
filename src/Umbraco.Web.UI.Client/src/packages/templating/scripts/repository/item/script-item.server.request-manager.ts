/* eslint-disable local-rules/no-direct-api-import */
import { scriptItemCache } from './script-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ScriptService, type ScriptItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiScriptItemDataRequestManager extends UmbManagementApiItemDataRequestManager<ScriptItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (paths: Array<string>) => ScriptService.getItemScript({ query: { path: paths } }),
			dataCache: scriptItemCache,
			getUniqueMethod: (item) => item.path,
		});
	}
}
