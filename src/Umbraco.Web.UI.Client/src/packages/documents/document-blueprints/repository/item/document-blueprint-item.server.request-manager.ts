/* eslint-disable local-rules/no-direct-api-import */
import { documentBlueprintItemCache } from './document-blueprint-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	DocumentBlueprintService,
	type DocumentBlueprintItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDocumentBlueprintItemDataRequestManager extends UmbManagementApiItemDataRequestManager<DocumentBlueprintItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<DocumentBlueprintItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => DocumentBlueprintService.getItemDocumentBlueprint({ query: { id: ids } }),
			dataCache: documentBlueprintItemCache,
			inflightRequestCache: UmbManagementApiDocumentBlueprintItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
