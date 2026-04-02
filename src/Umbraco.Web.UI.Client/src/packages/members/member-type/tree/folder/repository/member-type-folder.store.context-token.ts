import type { UmbMemberTypeFolderStore } from './member-type-folder.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEMBER_TYPE_FOLDER_STORE_CONTEXT = new UmbContextToken<UmbMemberTypeFolderStore>(
	'UmbMemberTypeFolderStore',
);
