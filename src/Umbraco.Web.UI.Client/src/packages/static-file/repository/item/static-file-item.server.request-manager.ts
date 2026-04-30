/* eslint-disable local-rules/no-direct-api-import */
import { staticFileItemCache } from './static-file-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { StaticFileService, type StaticFileItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiStaticFileItemDataRequestManager extends UmbManagementApiItemDataRequestManager<StaticFileItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<StaticFileItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (paths: Array<string>) => StaticFileService.getItemStaticFile({ query: { path: paths } }),
			dataCache: staticFileItemCache,
			inflightRequestCache: UmbManagementApiStaticFileItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.path,
		});
	}
}
