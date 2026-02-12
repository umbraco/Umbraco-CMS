import { mediaItemCache } from './media-item.server.cache.js';
import {
	UmbManagementApiItemDataCacheInvalidationManager,
	type UmbManagementApiServerEventModel,
} from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { MediaItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiMediaItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<MediaItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: mediaItemCache,
			/* The Media item model includes info about the Media Type.
			We need to invalidate the cache for both Media and MediaType events. */
			eventSources: ['Umbraco:CMS:Media', 'Umbraco:CMS:MediaType'],
			eventTypes: ['Updated', 'Deleted', 'Trashed'],
		});
	}

	protected override _onServerEvent(event: UmbManagementApiServerEventModel) {
		if (event.eventSource === 'Umbraco:CMS:MediaType') {
			this.#onMediaTypeChange(event);
		} else {
			this.#onMediaChange(event);
		}
	}

	#onMediaChange(event: UmbManagementApiServerEventModel) {
		// Invalidate the specific media item
		const mediaId = event.key;
		this._dataCache.delete(mediaId);
	}

	#onMediaTypeChange(event: UmbManagementApiServerEventModel) {
		// Invalidate all media items of the specified Media Type
		const mediaTypeId = event.key;
		const mediaIds = this._dataCache
			.getAll()
			.filter((cachedItem) => cachedItem.mediaType.id === mediaTypeId)
			.map((item) => item.id);

		mediaIds.forEach((id) => this._dataCache.delete(id));
	}
}
