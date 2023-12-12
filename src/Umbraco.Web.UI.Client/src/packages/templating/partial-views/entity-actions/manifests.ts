import {
	UMB_PARTIAL_VIEW_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
} from '../entity.js';
import { UMB_PARTIAL_VIEW_REPOSITORY_ALIAS } from '../repository/index.js';

import { UmbCreateFromSnippetPartialViewAction } from './create/create-from-snippet.action.js';
import { UmbCreateEmptyPartialViewAction } from './create/create-empty.action.js';
import {
	UmbCreateFolderEntityAction,
	UmbDeleteEntityAction,
	UmbDeleteFolderEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

//Actions for partial view files
const partialViewActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialView.Delete',
		name: 'Delete PartialView Entity Action',
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete...',
			repositoryAlias: UMB_PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_ENTITY_TYPE],
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
		api: UmbCreateEmptyPartialViewAction,
		meta: {
			icon: 'icon-article',
			label: 'New empty partial view',
			repositoryAlias: UMB_PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE, UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialViewFolder.Create.From.Snippet',
		name: 'Create PartialView Entity From Snippet Under Directory Action',
		api: UmbCreateFromSnippetPartialViewAction,
		meta: {
			icon: 'icon-article',
			label: 'New partial view from snippet...',
			repositoryAlias: UMB_PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE, UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialViewFolder.DeleteFolder',
		name: 'Delete Partial View Folder',
		api: UmbDeleteFolderEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete folder...',
			repositoryAlias: UMB_PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialViewFolder.CreateFolder',
		name: 'Create Partial View folder',
		api: UmbCreateFolderEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create folder...',
			repositoryAlias: UMB_PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE, UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE],
		},
	},
];

const createFromSnippetActionModal = {
	type: 'modal',
	alias: 'Umb.Modal.CreateFromSnippetPartialView',
	name: 'Choose insert type sidebar',
	js: () => import('./create/create-from-snippet.modal.js'),
};

export const manifests = [...partialViewActions, ...partialViewFolderActions, createFromSnippetActionModal];
