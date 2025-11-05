/* eslint-disable local-rules/no-direct-api-import */
import { mediaItemCache } from './media-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaService, type MediaItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMediaItemDataRequestManager extends UmbManagementApiItemDataRequestManager<MediaItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => MediaService.getItemMedia({ query: { id: ids } }),
			dataCache: mediaItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
