import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbScriptItemStore } from './script-item.store.js';

export const UMB_SCRIPT_ITEM_STORE_CONTEXT = new UmbContextToken<UmbScriptItemStore>('UmbScriptItemStore');
