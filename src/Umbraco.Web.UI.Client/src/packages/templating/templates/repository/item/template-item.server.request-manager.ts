/* eslint-disable local-rules/no-direct-api-import */
import { templateItemCache } from './template-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { TemplateService, type TemplateItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiTemplateItemDataRequestManager extends UmbManagementApiItemDataRequestManager<TemplateItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => TemplateService.getItemTemplate({ query: { id: ids } }),
			dataCache: templateItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
