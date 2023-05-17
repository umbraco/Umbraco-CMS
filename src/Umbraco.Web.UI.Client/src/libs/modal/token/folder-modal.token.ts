import { UmbModalToken } from 'src/libs/modal';
import { FolderReponseModel } from 'src/libs/backend-api';

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
