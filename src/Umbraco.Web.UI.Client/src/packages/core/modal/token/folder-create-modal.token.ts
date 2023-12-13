import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UmbFolderModel } from '@umbraco-cms/backoffice/repository';

export interface UmbFolderCreateModalData {
	folderRepositoryAlias: string;
	parentUnique: string | null;
}

export interface UmbFolderCreateModalValue {
	folder: UmbFolderModel;
}

export const UMB_FOLDER_CREATE_MODAL = new UmbModalToken<UmbFolderCreateModalData, UmbFolderCreateModalValue>(
	'Umb.Modal.Folder.Create',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
