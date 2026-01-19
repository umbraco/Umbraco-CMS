import { elementItemCache } from './element-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { ElementItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbManagementApiServerEventModel } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiElementItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<ElementItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: elementItemCache,
			/* The element item model includes info about the element Type.
			We need to invalidate the cache for both element and DocumentType events. */
			eventSources: ['Umbraco:CMS:Element', 'Umbraco:CMS:DocumentType'],
			eventTypes: ['Updated', 'Deleted', 'Trashed'],
		});
	}

	protected override _onServerEvent(event: UmbManagementApiServerEventModel) {
		if (event.eventSource === 'Umbraco:CMS:DocumentType') {
			this.#onElementTypeChange(event);
		} else {
			this.#onElementChange(event);
		}
	}

	#onElementChange(event: UmbManagementApiServerEventModel) {
		// Invalidate the specific element
		const elementId = event.key;
		this._dataCache.delete(elementId);
	}

	#onElementTypeChange(event: UmbManagementApiServerEventModel) {
		// Invalidate all elements of the specified element Type
		const elementTypeId = event.key;
		const elementIds = this._dataCache
			.getAll()
			.filter((cachedItem) => cachedItem.documentType.id === elementTypeId)
			.map((item) => item.id);

		elementIds.forEach((id) => this._dataCache.delete(id));
	}
}
