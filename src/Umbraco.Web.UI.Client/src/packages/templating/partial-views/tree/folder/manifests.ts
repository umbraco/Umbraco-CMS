import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE, UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbPartialViewFolderRepository } from './partial-view-folder.repository.js';
import { UmbCreateFolderEntityAction, UmbDeleteFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS,
	name: 'Partial View Folder Repository',
	api: UmbPartialViewFolderRepository,
};

export const UMB_DELETE_PARTIAL_VIEW_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.PartialView.Folder.Delete';
export const UMB_CREATE_PARTIAL_VIEW_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.PartialView.Folder.Create';

const entityActions = [
	{
		type: 'entityAction',
		alias: UMB_CREATE_PARTIAL_VIEW_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Create Partial View folder Entity Action',
		api: UmbCreateFolderEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create folder...',
			repositoryAlias: UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: UMB_DELETE_PARTIAL_VIEW_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Delete Partial View folder Entity Action',
		api: UmbDeleteFolderEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete folder...',
			repositoryAlias: UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
		},
	},
];

export const manifests = [folderRepository, ...entityActions];
