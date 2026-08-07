import { elementDetailCache } from './element-detail.server.cache.js';
import { UmbManagementApiDetailDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ElementResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbManagementApiServerEventModel } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiElementDetailDataCacheInvalidationManager extends UmbManagementApiDetailDataCacheInvalidationManager<ElementResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: elementDetailCache,
			/* The element detail model includes info about the element Type.
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
		this._dataCache.delete(event.key);
	}

	#onElementTypeChange(event: UmbManagementApiServerEventModel) {
		const elementTypeId = event.key;
		this._dataCache
			.getAll()
			.filter((cachedItem) => cachedItem.documentType.id === elementTypeId)
			.forEach((item) => this._dataCache.delete(item.id));
	}
}
