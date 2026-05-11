import type { UmbElementFolderStore } from './element-folder.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ELEMENT_FOLDER_STORE_CONTEXT = new UmbContextToken<UmbElementFolderStore>('UmbElementFolderStore');
