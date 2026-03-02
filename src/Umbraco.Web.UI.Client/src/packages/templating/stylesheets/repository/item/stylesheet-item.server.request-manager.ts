/* eslint-disable local-rules/no-direct-api-import */
import { stylesheetItemCache } from './stylesheet-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { StylesheetService, type StylesheetItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiStylesheetItemDataRequestManager extends UmbManagementApiItemDataRequestManager<StylesheetItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<StylesheetItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (paths: Array<string>) => StylesheetService.getItemStylesheet({ query: { path: paths } }),
			dataCache: stylesheetItemCache,
			inflightRequestCache: UmbManagementApiStylesheetItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.path,
		});
	}
}
