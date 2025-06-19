import type { UmbScriptFolderStore } from './script-folder.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SCRIPT_FOLDER_STORE_CONTEXT = new UmbContextToken<UmbScriptFolderStore>('UmbScriptFolderStore');
