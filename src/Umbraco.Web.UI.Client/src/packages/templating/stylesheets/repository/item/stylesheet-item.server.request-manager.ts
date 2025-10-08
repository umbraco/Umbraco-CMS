/* eslint-disable local-rules/no-direct-api-import */
import { stylesheetItemCache } from './stylesheet-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { StylesheetService, type StylesheetItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiStylesheetItemDataRequestManager extends UmbManagementApiItemDataRequestManager<StylesheetItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (paths: Array<string>) => StylesheetService.getItemStylesheet({ query: { path: paths } }),
			dataCache: stylesheetItemCache,
			getUniqueMethod: (item) => item.path,
		});
	}
}
