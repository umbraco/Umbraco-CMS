/* eslint-disable local-rules/no-direct-api-import */
import { languageItemCache } from './language-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { LanguageService, type LanguageItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiLanguageItemDataRequestManager extends UmbManagementApiItemDataRequestManager<LanguageItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (isoCodes: Array<string>) => LanguageService.getItemLanguage({ query: { isoCode: isoCodes } }),
			dataCache: languageItemCache,
			getUniqueMethod: (item) => item.isoCode,
		});
	}
}
