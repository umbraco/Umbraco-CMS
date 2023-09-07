import {
	STYLESHEET_ENTITY_TYPE,
	STYLESHEET_FOLDER_EMPTY_ENTITY_TYPE,
	STYLESHEET_FOLDER_ENTITY_TYPE,
	STYLESHEET_REPOSITORY_ALIAS,
	STYLESHEET_ROOT_ENTITY_TYPE,
} from '../config.js';
import { UmbCreateRTFStylesheetAction } from './create/create-rtf.action.js';
import { UmbCreateStylesheetAction } from './create/create.action.js';
import {
	UmbCreateFolderEntityAction,
	UmbDeleteEntityAction,
	UmbDeleteFolderEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

//TODO: this is temporary until we have a proper way of registering actions for folder types in a specific tree

//Actions for partial view files
const stylesheetActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.Delete',
		name: 'Delete Stylesheet Entity Action',
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
			repositoryAlias: STYLESHEET_REPOSITORY_ALIAS,
			entityTypes: [STYLESHEET_ENTITY_TYPE],
		},
	},
];

//TODO: add create folder action when the generic folder action is implemented
//Actions for directories
const stylesheetFolderActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.Folder.Create',
		name: 'Create Stylesheet Entity Under Directory Action',
		meta: {
			icon: 'umb:script',
			label: 'New stylesheet file',
			api: UmbCreateStylesheetAction,
			repositoryAlias: STYLESHEET_REPOSITORY_ALIAS,
			entityTypes: [STYLESHEET_FOLDER_ENTITY_TYPE, STYLESHEET_FOLDER_EMPTY_ENTITY_TYPE, STYLESHEET_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.Folder.Create.RTF',
		name: 'Create RTF Stylesheet Entity Under Directory Action',
		meta: {
			icon: 'umb:script',
			label: 'New Rich Text Editor style sheet file',
			api: UmbCreateRTFStylesheetAction,
			repositoryAlias: STYLESHEET_REPOSITORY_ALIAS,
			entityTypes: [STYLESHEET_FOLDER_ENTITY_TYPE, STYLESHEET_FOLDER_EMPTY_ENTITY_TYPE, STYLESHEET_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.Folder.DeleteFolder',
		name: 'Remove empty folder',
		meta: {
			icon: 'umb:trash',
			label: 'Remove folder',
			api: UmbDeleteFolderEntityAction,
			repositoryAlias: STYLESHEET_REPOSITORY_ALIAS,
			entityTypes: [STYLESHEET_FOLDER_EMPTY_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.Folder.CreateFolder',
		name: 'Create empty folder',
		meta: {
			icon: 'umb:add',
			label: 'Create folder',
			api: UmbCreateFolderEntityAction,
			repositoryAlias: STYLESHEET_REPOSITORY_ALIAS,
			entityTypes: [STYLESHEET_FOLDER_EMPTY_ENTITY_TYPE, STYLESHEET_FOLDER_ENTITY_TYPE, STYLESHEET_ROOT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...stylesheetActions, ...stylesheetFolderActions];
