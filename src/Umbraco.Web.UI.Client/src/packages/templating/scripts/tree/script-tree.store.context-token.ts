import type { UmbScriptTreeStore } from './script-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SCRIPT_TREE_STORE_CONTEXT = new UmbContextToken<UmbScriptTreeStore>('UmbScriptTreeStore');
