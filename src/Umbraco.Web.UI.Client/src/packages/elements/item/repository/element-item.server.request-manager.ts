/* eslint-disable local-rules/no-direct-api-import */
import { elementItemCache } from './element-item.server.cache.js';
import { ElementService, type ElementItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbManagementApiElementItemDataRequestManager extends UmbManagementApiItemDataRequestManager<ElementItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => ElementService.getItemElement({ query: { id: ids } }),
			dataCache: elementItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
