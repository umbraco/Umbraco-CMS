import type { UmbTemplateTreeStore } from './template-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * @deprecated - Use `UmbTemplateTreeRepository` instead. This will be removed in Umbraco 18.
 */
export const UMB_TEMPLATE_TREE_STORE_CONTEXT = new UmbContextToken<UmbTemplateTreeStore>('UmbTemplateTreeStore');
