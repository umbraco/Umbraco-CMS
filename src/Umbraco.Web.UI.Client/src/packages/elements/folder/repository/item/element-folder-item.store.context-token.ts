import type { UmbElementFolderItemStore } from './element-folder-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ELEMENT_FOLDER_ITEM_STORE_CONTEXT = new UmbContextToken<UmbElementFolderItemStore>(
	'UmbElementFolderItemStore',
);
