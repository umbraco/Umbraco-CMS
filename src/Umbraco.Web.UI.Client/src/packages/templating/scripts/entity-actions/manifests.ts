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
		alias: 'Umb.EntityAction.PartialView.Delete',
		name: 'Delete PartialView Entity Action',
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
		alias: 'Umb.EntityAction.PartialViewFolder.Create.New',
		name: 'Create PartialView Entity Under Directory Action',
		meta: {
			icon: 'umb:article',
			label: 'New empty partial view',
			api: UmbCreateScriptAction,
			repositoryAlias: SCRIPTS_REPOSITORY_ALIAS,
			entityTypes: [SCRIPTS_FOLDER_ENTITY_TYPE, SCRIPTS_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialViewFolder.DeleteFolder',
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
		alias: 'Umb.EntityAction.PartialViewFolder.CreateFolder',
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
