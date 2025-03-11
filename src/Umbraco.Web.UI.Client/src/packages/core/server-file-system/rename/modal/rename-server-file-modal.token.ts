import { UMB_RENAME_SERVER_FILE_MODAL_ALIAS } from './manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbRenameModalData {
	renameRepositoryAlias: string;
	itemRepositoryAlias: string;
	unique: string;
}

export interface UmbRenameServerFileModalValue {
	unique: string;
	name: string;
}

export const UMB_RENAME_SERVER_FILE_MODAL = new UmbModalToken<UmbRenameModalData, UmbRenameServerFileModalValue>(
	UMB_RENAME_SERVER_FILE_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
