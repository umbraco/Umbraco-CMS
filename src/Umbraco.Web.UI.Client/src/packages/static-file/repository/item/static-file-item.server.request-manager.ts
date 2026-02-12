/* eslint-disable local-rules/no-direct-api-import */
import { staticFileItemCache } from './static-file-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { StaticFileService, type StaticFileItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiStaticFileItemDataRequestManager extends UmbManagementApiItemDataRequestManager<StaticFileItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (paths: Array<string>) => StaticFileService.getItemStaticFile({ query: { path: paths } }),
			dataCache: staticFileItemCache,
			getUniqueMethod: (item) => item.path,
		});
	}
}
