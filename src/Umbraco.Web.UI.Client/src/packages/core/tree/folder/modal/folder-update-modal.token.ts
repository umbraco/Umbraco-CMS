import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export interface UmbFolderUpdateModalData {
	folderRepositoryAlias: string;
	unique: string;
}

export interface UmbFolderUpdateModalValue {
	folder: UmbFolderModel;
}

export const UMB_FOLDER_UPDATE_MODAL = new UmbModalToken<UmbFolderUpdateModalData, UmbFolderUpdateModalValue>(
	'Umb.Modal.Folder.Update',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
