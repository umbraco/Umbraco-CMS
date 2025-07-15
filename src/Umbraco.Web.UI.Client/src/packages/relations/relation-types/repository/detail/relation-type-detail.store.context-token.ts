import type UmbRelationTypeDetailStore from './relation-type-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_RELATION_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbRelationTypeDetailStore>(
	'UmbRelationTypeDetailStore',
);
