import type { UmbDataTypeItemModel } from '../item/types.js';
import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../item/constants.js';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDataTypeItemRepositoryManager extends UmbRepositoryItemsManager<UmbDataTypeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS);
	}
}
