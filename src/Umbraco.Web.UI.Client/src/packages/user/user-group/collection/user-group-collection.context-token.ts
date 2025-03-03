import type { UmbUserGroupCollectionContext } from './user-group-collection.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_USER_GROUP_COLLECTION_CONTEXT = new UmbContextToken<UmbUserGroupCollectionContext>(
	'UmbCollectionContext',
);
