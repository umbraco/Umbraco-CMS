import type { UmbMediaTypeFolderStore } from './media-type-folder.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEDIA_TYPE_FOLDER_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeFolderStore>(
	'UmbMediaTypeFolderStore',
);
