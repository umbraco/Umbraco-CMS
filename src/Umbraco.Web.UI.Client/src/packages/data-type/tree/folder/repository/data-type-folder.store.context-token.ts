import type { UmbDataTypeFolderStore } from './data-type-folder.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DATA_TYPE_FOLDER_STORE_CONTEXT = new UmbContextToken<UmbDataTypeFolderStore>('UmbDataTypeFolderStore');
