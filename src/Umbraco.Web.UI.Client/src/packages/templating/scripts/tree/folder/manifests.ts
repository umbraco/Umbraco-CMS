import { UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbScriptFolderRepository } from './script-folder.repository.js';
import { UmbCreateFolderEntityAction, UmbDeleteFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS = 'Umb.Repository.Script.Folder';

const folderRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
	name: 'Script Folder Repository',
	api: UmbScriptFolderRepository,
};

export const UMB_DELETE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Folder.Delete';
export const UMB_CREATE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Folder.Create';

const entityActions = [
	{
		type: 'entityAction',
		alias: UMB_CREATE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Create Script folder',
		api: UmbCreateFolderEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create folder...',
			repositoryAlias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: UMB_DELETE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Delete Script folder',
		api: UmbDeleteFolderEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete folder...',
			repositoryAlias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		},
	},
];

export const manifests = [folderRepository, ...entityActions];
