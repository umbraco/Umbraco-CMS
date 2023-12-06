import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { FolderResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderModalData {
	repositoryAlias: string;
	unique?: string;
	parentUnique?: string | null;
}

export interface UmbFolderModalValue {
	folder: FolderResponseModel;
}

export const UMB_FOLDER_MODAL = new UmbModalToken<UmbFolderModalData, UmbFolderModalValue>('Umb.Modal.Folder', {
	config: {
		type: 'sidebar',
		size: 'small',
	},
});
