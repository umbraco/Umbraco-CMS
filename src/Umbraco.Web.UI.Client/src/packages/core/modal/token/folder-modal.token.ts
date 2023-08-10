import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { FolderResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderModalData {
	repositoryAlias: string;
	unique?: string;
	parentUnique?: string | null;
}

export interface UmbFolderModalResult {
	folder: FolderResponseModel;
}

export const UMB_FOLDER_MODAL = new UmbModalToken<UmbFolderModalData, UmbFolderModalResult>('Umb.Modal.Folder', {
	type: 'sidebar',
	size: 'small',
});
