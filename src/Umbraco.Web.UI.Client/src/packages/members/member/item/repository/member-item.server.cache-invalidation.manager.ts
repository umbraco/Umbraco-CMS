import { memberItemCache } from './member-item.server.cache.js';
import {
	UmbManagementApiItemDataCacheInvalidationManager,
	type UmbManagementApiServerEventModel,
} from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { MemberItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiMemberItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<MemberItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: memberItemCache,
			/* The Member item model includes info about the Member Type. 
			We need to invalidate the cache for both Member and MemberType events. */
			eventSources: ['Umbraco:CMS:Member', 'Umbraco:CMS:MemberType'],
		});
	}

	protected override _onServerEvent(event: UmbManagementApiServerEventModel) {
		if (event.eventSource === 'Umbraco:CMS:MemberType') {
			this.#onMemberTypeChange(event);
		} else {
			this.#onMemberChange(event);
		}
	}

	#onMemberChange(event: UmbManagementApiServerEventModel) {
		// Invalidate the specific member
		const memberId = event.key;
		this._dataCache.delete(memberId);
	}

	#onMemberTypeChange(event: UmbManagementApiServerEventModel) {
		// Invalidate all members of the specified Member Type
		const memberTypeId = event.key;
		const memberIds = this._dataCache
			.getAll()
			.filter((cachedItem) => cachedItem.memberType.id === memberTypeId)
			.map((item) => item.id);

		memberIds.forEach((id) => this._dataCache.delete(id));
	}
}
