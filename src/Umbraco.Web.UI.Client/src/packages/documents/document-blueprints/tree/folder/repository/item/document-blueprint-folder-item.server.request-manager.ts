/* eslint-disable local-rules/no-direct-api-import */
import { documentBlueprintFolderItemCache } from './document-blueprint-folder-item.server.cache.js';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import { DocumentBlueprintService } from '@umbraco-cms/backoffice/external/backend-api';
import type { FolderItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbManagementApiDocumentBlueprintFolderItemDataRequestManager extends UmbManagementApiItemDataRequestManager<FolderItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => DocumentBlueprintService.getItemDocumentBlueprintFolder({ query: { id: ids } }),
			dataCache: documentBlueprintFolderItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
