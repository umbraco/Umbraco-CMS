import {
	SCRIPTS_REPOSITORY_ALIAS,
	SCRIPTS_ENTITY_TYPE,
	SCRIPTS_FOLDER_ENTITY_TYPE,
	SCRIPTS_ROOT_ENTITY_TYPE,
	SCRIPTS_FOLDER_EMPTY_ENTITY_TYPE,
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
		alias: 'Umb.EntityAction.Scripts.Delete',
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
		alias: 'Umb.EntityAction.ScriptsFolder.Create.New',
		name: 'Create Scripts Entity Under Directory Action',
		meta: {
			icon: 'umb:article',
			label: 'New empty script',
			api: UmbCreateScriptAction,
			repositoryAlias: SCRIPTS_REPOSITORY_ALIAS,
			entityTypes: [SCRIPTS_FOLDER_ENTITY_TYPE, SCRIPTS_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.ScriptsFolder.DeleteFolder',
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
		alias: 'Umb.EntityAction.ScriptsFolder.CreateFolder',
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
