import { elementItemCache } from './element-item.server.cache.js';
import {
	UmbManagementApiItemDataCacheInvalidationManager,
	type UmbManagementApiServerEventModel,
} from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ElementItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiElementItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<ElementItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: elementItemCache,
			/* The Element item model includes info about the Element Type.
			We need to invalidate the cache for both Element and DocumentType events. */
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
		// Invalidate the specific Element
		const ElementId = event.key;
		this._dataCache.delete(ElementId);
	}

	#onElementTypeChange(event: UmbManagementApiServerEventModel) {
		// Invalidate all Elements of the specified Element Type
		const ElementTypeId = event.key;
		const ElementIds = this._dataCache
			.getAll()
			.filter((cachedItem) => cachedItem.documentType.id === ElementTypeId)
			.map((item) => item.id);

		ElementIds.forEach((id) => this._dataCache.delete(id));
	}
}
