import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbScriptDetailStore } from './script-detail.store.js';

export const UMB_SCRIPT_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbScriptDetailStore>('UmbScriptDetailStore');
