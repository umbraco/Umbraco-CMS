import type { UmbStylesheetTreeStore } from './stylesheet-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * @deprecated - Use `UmbStylesheetTreeRepository` instead. This will be removed in Umbraco 18.
 */
export const UMB_STYLESHEET_TREE_STORE_CONTEXT = new UmbContextToken<UmbStylesheetTreeStore>('UmbStylesheetTreeStore');
