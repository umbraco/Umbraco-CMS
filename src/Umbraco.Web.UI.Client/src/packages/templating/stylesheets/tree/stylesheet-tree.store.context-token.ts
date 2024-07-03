import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbStylesheetTreeStore } from './stylesheet-tree.store.js';

export const UMB_STYLESHEET_TREE_STORE_CONTEXT = new UmbContextToken<UmbStylesheetTreeStore>('UmbStylesheetTreeStore');
