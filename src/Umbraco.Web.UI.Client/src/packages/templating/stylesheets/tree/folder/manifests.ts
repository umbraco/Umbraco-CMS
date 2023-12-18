import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE, UMB_STYLESHEET_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbCreateFolderEntityAction, UmbDeleteFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
	name: 'Stylesheet Folder Repository',
	api: UmbStylesheetFolderRepository,
};

export const UMB_DELETE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Stylesheet.Folder.Delete';
export const UMB_CREATE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Stylesheet.Folder.Create';

const entityActions = [
	{
		type: 'entityAction',
		alias: UMB_CREATE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Create Stylesheet folder Entity Action',
		api: UmbCreateFolderEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create folder...',
			repositoryAlias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: UMB_DELETE_STYLESHEET_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Delete Stylesheet folder Entity Action',
		api: UmbDeleteFolderEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete folder...',
			repositoryAlias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		},
	},
];

export const manifests = [folderRepository, ...entityActions];
