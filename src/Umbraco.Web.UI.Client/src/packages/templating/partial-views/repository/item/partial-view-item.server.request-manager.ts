/* eslint-disable local-rules/no-direct-api-import */
import { partialViewItemCache } from './partial-view-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { PartialViewService, type PartialViewItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiPartialViewItemDataRequestManager extends UmbManagementApiItemDataRequestManager<PartialViewItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<PartialViewItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (paths: Array<string>) => PartialViewService.getItemPartialView({ query: { path: paths } }),
			dataCache: partialViewItemCache,
			inflightRequestCache: UmbManagementApiPartialViewItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.path,
		});
	}
}
