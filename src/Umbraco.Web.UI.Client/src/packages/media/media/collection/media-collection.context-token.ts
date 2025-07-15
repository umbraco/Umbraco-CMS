import type { UmbMediaCollectionContext } from './media-collection.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEDIA_COLLECTION_CONTEXT = new UmbContextToken<UmbMediaCollectionContext>('UmbCollectionContext');
