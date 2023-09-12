import {
	SCRIPTS_REPOSITORY_ALIAS,
	SCRIPTS_ENTITY_TYPE,
	SCRIPTS_FOLDER_ENTITY_TYPE,
	SCRIPTS_ROOT_ENTITY_TYPE,
	SCRIPTS_FOLDER_EMPTY_ENTITY_TYPE,
	SCRIPTS_ENTITY_ACTION_DELETE_ALIAS,
	SCRIPTS_ENTITY_ACTION_CREATE_NEW_ALIAS,
	SCRIPTS_ENTITY_ACTION_DELETE_FOLDER_ALIAS,
	SCRIPTS_ENTITY_ACTION_CREATE_FOLDER_NEW_ALIAS,
} from '../config.js';
import { UmbCreateScriptAction } from './create/create-empty.action.js';
import {
	UmbCreateFolderEntityAction,
	UmbDeleteEntityAction,
	UmbDeleteFolderEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const scriptsViewActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: SCRIPTS_ENTITY_ACTION_DELETE_ALIAS,
		name: 'Delete Scripts Entity Action',
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
			repositoryAlias: SCRIPTS_REPOSITORY_ALIAS,
			entityTypes: [SCRIPTS_ENTITY_TYPE],
		},
	},
];

const scriptsFolderActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: SCRIPTS_ENTITY_ACTION_CREATE_NEW_ALIAS,
		name: 'Create Scripts Entity Under Directory Action',
		meta: {
			icon: 'umb:article',
			label: 'New empty script',
			api: UmbCreateScriptAction,
			repositoryAlias: SCRIPTS_REPOSITORY_ALIAS,
			entityTypes: [SCRIPTS_FOLDER_ENTITY_TYPE, SCRIPTS_FOLDER_EMPTY_ENTITY_TYPE, SCRIPTS_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: SCRIPTS_ENTITY_ACTION_DELETE_FOLDER_ALIAS,
		name: 'Remove empty folder',
		meta: {
			icon: 'umb:trash',
			label: 'Remove folder',
			api: UmbDeleteFolderEntityAction,
			repositoryAlias: SCRIPTS_REPOSITORY_ALIAS,
			entityTypes: [SCRIPTS_FOLDER_EMPTY_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: SCRIPTS_ENTITY_ACTION_CREATE_FOLDER_NEW_ALIAS,
		name: 'Create empty folder',
		meta: {
			icon: 'umb:add',
			label: 'Create folder',
			api: UmbCreateFolderEntityAction,
			repositoryAlias: SCRIPTS_REPOSITORY_ALIAS,
			entityTypes: [SCRIPTS_FOLDER_EMPTY_ENTITY_TYPE, SCRIPTS_FOLDER_ENTITY_TYPE, SCRIPTS_ROOT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...scriptsViewActions, ...scriptsFolderActions];
