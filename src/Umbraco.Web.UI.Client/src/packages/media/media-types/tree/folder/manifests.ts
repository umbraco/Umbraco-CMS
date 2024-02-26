import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UmbDeleteFolderEntityAction, UmbFolderUpdateEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestEntityAction, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
	name: 'Media Type Folder Repository',
	api: () => import('./media-type-folder.repository.js'),
};

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.RenameFolder',
		name: 'Rename Media Type Folder Entity Action',
		weight: 800,
		api: UmbFolderUpdateEntityAction,
		meta: {
			icon: 'icon-edit',
			label: 'Rename Folder...',
			repositoryAlias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.DeleteFolder',
		name: 'Delete Media Type Folder Entity Action',
		weight: 700,
		api: UmbDeleteFolderEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete Folder...',
			repositoryAlias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
		},
	},
];

export const manifests = [folderRepository, ...entityActions];
