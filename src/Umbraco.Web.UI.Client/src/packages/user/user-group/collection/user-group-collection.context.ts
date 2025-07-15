import type { UmbUserGroupDetailModel } from '../types.js';
import type { UmbUserGroupCollectionFilterModel } from './types.js';
import { UMB_USER_GROUP_TABLE_COLLECTION_VIEW_ALIAS } from './views/constants.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserGroupCollectionContext extends UmbDefaultCollectionContext<
	UmbUserGroupDetailModel,
	UmbUserGroupCollectionFilterModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_GROUP_TABLE_COLLECTION_VIEW_ALIAS);
	}
}

export { UmbUserGroupCollectionContext as api };
