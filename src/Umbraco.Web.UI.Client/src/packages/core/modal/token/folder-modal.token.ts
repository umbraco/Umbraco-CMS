import { UmbModalToken } from 'src/packages/core/modal';
import { FolderReponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderModalData {
	repositoryAlias: string;
	unique?: string;
}

export interface UmbFolderModalResult {
	folder: FolderReponseModel;
}

export const UMB_FOLDER_MODAL = new UmbModalToken<UmbFolderModalData, UmbFolderModalResult>('Umb.Modal.Folder', {
	type: 'sidebar',
	size: 'small',
});
