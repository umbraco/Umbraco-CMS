/* eslint-disable local-rules/no-direct-api-import */
import { scriptItemCache } from './script-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ScriptService, type ScriptItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiScriptItemDataRequestManager extends UmbManagementApiItemDataRequestManager<ScriptItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<ScriptItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (paths: Array<string>) => ScriptService.getItemScript({ query: { path: paths } }),
			dataCache: scriptItemCache,
			inflightRequestCache: UmbManagementApiScriptItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.path,
		});
	}
}
