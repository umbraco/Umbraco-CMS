import type { UmbMemberCollectionContext } from './member-collection.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEMBER_COLLECTION_CONTEXT = new UmbContextToken<UmbMemberCollectionContext>('UmbCollectionContext');
