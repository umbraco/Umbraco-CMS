import type { UmbUserCollectionContext } from './user-collection.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_USER_COLLECTION_CONTEXT = new UmbContextToken<UmbUserCollectionContext>('UmbCollectionContext');
