import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbTemplateTreeStore } from './template-tree.store.js';

export const UMB_TEMPLATE_TREE_STORE_CONTEXT = new UmbContextToken<UmbTemplateTreeStore>('UmbTemplateTreeStore');
