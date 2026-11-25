import type { UmbRelationTypeItemStore } from './relation-type-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_RELATION_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbRelationTypeItemStore>(
	'UmbRelationTypeItemStore',
);
