import type { UmbStylesheetFolderStore } from './stylesheet-folder.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_STYLESHEET_FOLDER_STORE_CONTEXT = new UmbContextToken<UmbStylesheetFolderStore>(
	'UmbStylesheetFolderStore',
);
