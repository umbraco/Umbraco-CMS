import type { UmbPartialViewFolderStore } from './partial-view-folder.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PARTIAL_VIEW_FOLDER_STORE_CONTEXT = new UmbContextToken<UmbPartialViewFolderStore>(
	'UmbPartialViewFolderStore',
);
