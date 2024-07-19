import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export interface UmbFolderCreateModalData {
	folderRepositoryAlias: string;
	parent: UmbEntityModel;
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
