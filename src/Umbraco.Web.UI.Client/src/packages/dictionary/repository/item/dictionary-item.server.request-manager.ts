/* eslint-disable local-rules/no-direct-api-import */
import { dictionaryItemCache } from './dictionary-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DictionaryService, type DictionaryItemItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDictionaryItemDataRequestManager extends UmbManagementApiItemDataRequestManager<DictionaryItemItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<DictionaryItemItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => DictionaryService.getItemDictionary({ query: { id: ids } }),
			dataCache: dictionaryItemCache,
			inflightRequestCache: UmbManagementApiDictionaryItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
