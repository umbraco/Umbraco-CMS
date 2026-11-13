/* eslint-disable local-rules/no-direct-api-import */
import { relationTypeItemCache } from './relation-type-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { RelationTypeService, type RelationTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiRelationTypeItemDataRequestManager extends UmbManagementApiItemDataRequestManager<RelationTypeItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<RelationTypeItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => RelationTypeService.getItemRelationType({ query: { id: ids } }),
			dataCache: relationTypeItemCache,
			inflightRequestCache: UmbManagementApiRelationTypeItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
