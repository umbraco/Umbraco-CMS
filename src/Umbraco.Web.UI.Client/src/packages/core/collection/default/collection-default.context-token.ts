import type { UmbDefaultCollectionContext } from './collection-default.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_COLLECTION_CONTEXT = new UmbContextToken<UmbDefaultCollectionContext>('UmbCollectionContext');
