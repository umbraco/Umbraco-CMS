import {
	PARTIAL_VIEW_ENTITY_TYPE,
	PARTIAL_VIEW_FOLDER_EMPTY_ENTITY_TYPE,
	PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
	PARTIAL_VIEW_REPOSITORY_ALIAS,
	PARTIAL_VIEW_ROOT_ENTITY_TYPE,
} from '../config.js';
import { UmbCreateFromSnippetPartialViewAction } from './create/create-from-snippet.action.js';
import { UmbCreateEmptyPartialViewAction } from './create/create-empty.action.js';
import {
	UmbCreateFolderEntityAction,
	UmbDeleteEntityAction,
	UmbDeleteFolderEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

//TODO: this is temporary until we have a proper way of registering actions for folder types in a specific tree

//Actions for partial view files
const partialViewActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialView.Delete',
		name: 'Delete PartialView Entity Action',
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			api: UmbDeleteEntityAction,
			repositoryAlias: PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [PARTIAL_VIEW_ENTITY_TYPE],
		},
	},
];

//TODO: add create folder action when the generic folder action is implemented
//Actions for directories
const partialViewFolderActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialViewFolder.Create.New',
		name: 'Create PartialView Entity Under Directory Action',
		meta: {
			icon: 'umb:article',
			label: 'New empty partial view',
			api: UmbCreateEmptyPartialViewAction,
			repositoryAlias: PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [PARTIAL_VIEW_FOLDER_ENTITY_TYPE, PARTIAL_VIEW_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialViewFolder.Create.From.Snippet',
		name: 'Create PartialView Entity From Snippet Under Directory Action',
		meta: {
			icon: 'umb:article',
			label: 'New partial view from snippet...',
			api: UmbCreateFromSnippetPartialViewAction,
			repositoryAlias: PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [PARTIAL_VIEW_FOLDER_ENTITY_TYPE, PARTIAL_VIEW_ROOT_ENTITY_TYPE],
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
			repositoryAlias: PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [PARTIAL_VIEW_FOLDER_EMPTY_ENTITY_TYPE],
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
			repositoryAlias: PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [
				PARTIAL_VIEW_FOLDER_EMPTY_ENTITY_TYPE,
				PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
				PARTIAL_VIEW_ROOT_ENTITY_TYPE,
			],
		},
	},
];

const createFromSnippetActionModal = {
	type: 'modal',
	alias: 'Umb.Modal.CreateFromSnippetPartialView',
	name: 'Choose insert type sidebar',
	loader: () => import('./create/create-from-snippet.modal.js'),
};

export const manifests = [...partialViewActions, ...partialViewFolderActions, createFromSnippetActionModal];
