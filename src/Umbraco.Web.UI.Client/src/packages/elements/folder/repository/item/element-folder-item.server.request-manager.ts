/* eslint-disable local-rules/no-direct-api-import */
import { elementFolderItemCache } from './element-folder-item.server.cache.js';
import { ElementService, type FolderItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbManagementApiElementFolderItemDataRequestManager extends UmbManagementApiItemDataRequestManager<FolderItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => ElementService.getItemElementFolder({ query: { id: ids } }),
			dataCache: elementFolderItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
