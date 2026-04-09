import type { UmbUserGroupDetailModel } from '../types.js';
import type { UmbUserGroupCollectionFilterModel } from './types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * @deprecated The context token is deprecated. Use UMB_COLLECTION_CONTEXT instead.
 */
export const UMB_USER_GROUP_COLLECTION_CONTEXT = new UmbContextToken<
	UmbDefaultCollectionContext<UmbUserGroupDetailModel, UmbUserGroupCollectionFilterModel>
>('UmbCollectionContext');
