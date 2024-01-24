import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UmbDataTypeFolderRepository } from './data-type-folder.repository.js';
import { UmbDeleteFolderEntityAction, UmbFolderUpdateEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
	name: 'Data Type Folder Repository',
	api: UmbDataTypeFolderRepository,
};

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.RenameFolder',
		name: 'Rename Data Type Folder Entity Action',
		weight: 800,
		api: UmbFolderUpdateEntityAction,
		meta: {
			icon: 'icon-edit',
			label: 'Rename Folder...',
			repositoryAlias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_DATA_TYPE_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.DeleteFolder',
		name: 'Delete Data Type Folder Entity Action',
		weight: 700,
		api: UmbDeleteFolderEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete Folder...',
			repositoryAlias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_DATA_TYPE_FOLDER_ENTITY_TYPE],
		},
	},
];

export const manifests = [folderRepository, ...entityActions];
