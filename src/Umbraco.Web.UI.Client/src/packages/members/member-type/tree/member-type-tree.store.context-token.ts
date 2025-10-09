import type { UmbMemberTypeTreeStore } from './member-type-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * @deprecated - Use `UmbMemberTypeTreeRepository` instead. This will be removed in Umbraco 18.
 */
export const UMB_MEMBER_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbMemberTypeTreeStore>('UmbMemberTypeTreeStore');
