import type { UmbPartialViewItemModel } from '../../types.js';
import { UmbPartialViewItemServerDataSource } from './partial-view-item.server.data-source.js';
import { UMB_PARTIAL_VIEW_ITEM_STORE_CONTEXT } from './partial-view-item.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbPartialViewItemRepository extends UmbItemRepositoryBase<UmbPartialViewItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbPartialViewItemServerDataSource, UMB_PARTIAL_VIEW_ITEM_STORE_CONTEXT);
	}
}

export default UmbPartialViewItemRepository;
