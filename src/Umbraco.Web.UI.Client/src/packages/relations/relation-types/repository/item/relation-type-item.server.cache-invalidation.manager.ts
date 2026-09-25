import { relationTypeItemCache } from './relation-type-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { RelationTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiRelationTypeItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<RelationTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: relationTypeItemCache,
			eventSources: ['Umbraco:CMS:RelationType'],
		});
	}
}
