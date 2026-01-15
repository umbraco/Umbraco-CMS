import { documentItemCache } from './document-item.server.cache.js';
import {
	UmbManagementApiItemDataCacheInvalidationManager,
	type UmbManagementApiServerEventModel,
} from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiDocumentItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<DocumentItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: documentItemCache,
			/* The Document item model includes info about the Document Type.
			We need to invalidate the cache for both Document and DocumentType events. */
			eventSources: ['Umbraco:CMS:Document', 'Umbraco:CMS:DocumentType'],
			eventTypes: ['Updated', 'Deleted', 'Trashed'],
		});
	}

	protected override _onServerEvent(event: UmbManagementApiServerEventModel) {
		if (event.eventSource === 'Umbraco:CMS:DocumentType') {
			this.#onDocumentTypeChange(event);
		} else {
			this.#onDocumentChange(event);
		}
	}

	#onDocumentChange(event: UmbManagementApiServerEventModel) {
		// Invalidate the specific document
		const documentId = event.key;
		this._dataCache.delete(documentId);
	}

	#onDocumentTypeChange(event: UmbManagementApiServerEventModel) {
		// Invalidate all documents of the specified Document Type
		const documentTypeId = event.key;
		const documentIds = this._dataCache
			.getAll()
			.filter((cachedItem) => cachedItem.documentType.id === documentTypeId)
			.map((item) => item.id);

		documentIds.forEach((id) => this._dataCache.delete(id));
	}
}
