import { UMB_SCRIPT_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbCreateScriptAction } from './create/create-empty.action.js';
import {
	UmbCreateFolderEntityAction,
	UmbDeleteEntityAction,
	UmbDeleteFolderEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DELETE_SCRIPT_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Delete';
export const UMB_CREATE_SCRIPT_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Create';
export const UMB_DELETE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Folder.Delete';
export const UMB_CREATE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Folder.Create';

const scriptViewActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: UMB_DELETE_SCRIPT_ENTITY_ACTION_ALIAS,
		name: 'Delete Script Entity Action',
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete',
			repositoryAlias: UMB_SCRIPT_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_ENTITY_TYPE],
		},
	},
];

const scriptFolderActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: UMB_CREATE_SCRIPT_ENTITY_ACTION_ALIAS,
		name: 'Create Script Under Directory Entity Action',
		api: UmbCreateScriptAction,
		meta: {
			icon: 'icon-article',
			label: 'New empty script',
			repositoryAlias: UMB_SCRIPT_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE],
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
			repositoryAlias: UMB_SCRIPT_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: UMB_CREATE_SCRIPT_FOLDER_ENTITY_ACTION_ALIAS,
		name: 'Create Script folder',
		api: UmbCreateFolderEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create folder...',
			repositoryAlias: UMB_SCRIPT_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...scriptViewActions, ...scriptFolderActions];
