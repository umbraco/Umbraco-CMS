import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { FolderResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderModalData {
	folderRepositoryAlias: string;
	unique?: string;
	parentUnique?: string | null;
}

export interface UmbFolderModalValue {
	folder: FolderResponseModel;
}

export const UMB_FOLDER_MODAL = new UmbModalToken<UmbFolderModalData, UmbFolderModalValue>('Umb.Modal.Folder', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
