import { UmbMediaItemServerDataSource } from './media-item.server.data-source.js';
import { UMB_MEDIA_ITEM_STORE_CONTEXT } from './media-item.store.js';
import type { UmbMediaItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMediaItemRepository extends UmbItemRepositoryBase<UmbMediaItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaItemServerDataSource, UMB_MEDIA_ITEM_STORE_CONTEXT);
	}
}
