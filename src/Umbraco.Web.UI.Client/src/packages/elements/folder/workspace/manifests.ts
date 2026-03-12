import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_MENU_ITEM_ALIAS } from '../../menu/constants.js';
import { UMB_ELEMENT_ROOT_WORKSPACE_ALIAS } from '../../workspace/element-root/constants.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_UPDATE,
} from '../../user-permissions/constants.js';
import { UMB_ELEMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspaceAction,
	ManifestWorkspaceFooterApp,
	ManifestWorkspaceRoutableKind,
} from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspaceViewCollectionKind } from '@umbraco-cms/backoffice/collection';
import type { ManifestWorkspaceContextMenuStructureKind } from '@umbraco-cms/backoffice/menu';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

const workspace: ManifestWorkspaceRoutableKind = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
	name: 'Element Folder Workspace',
	api: () => import('./element-folder-workspace.context.js'),
	meta: {
		entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
	},
};

const workspaceView: ManifestWorkspaceViewCollectionKind = {
	type: 'workspaceView',
	kind: 'collection',
	alias: 'Umb.WorkspaceView.Element.Collection',
	name: 'Element Collection Workspace View',
	meta: {
		label: 'Folder',
		pathname: 'folder',
		icon: 'icon-folder',
		collectionAlias: UMB_ELEMENT_COLLECTION_ALIAS,
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			oneOf: [UMB_ELEMENT_ROOT_WORKSPACE_ALIAS, UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS],
		},
		{
			alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
		},
	],
};

const workspaceAction: ManifestWorkspaceAction = {
	type: 'workspaceAction',
	kind: 'default',
	alias: 'Umb.WorkspaceAction.Element.Folder.Submit',
	name: 'Submit Element Folder Workspace Action',
	api: UmbSubmitWorkspaceAction,
	meta: {
		label: '#buttons_save',
		look: 'primary',
		color: 'positive',
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
		},
		{
			alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
			allOf: [UMB_USER_PERMISSION_ELEMENT_UPDATE],
		},
		{
			alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
		},
	],
};

const menuStructure: ManifestWorkspaceContextMenuStructureKind = {
	type: 'workspaceContext',
	kind: 'menuStructure',
	alias: 'Umb.Context.ElementFolder.Menu.Structure',
	name: 'Element Folder Menu Structure Workspace Context',
	api: () => import('./element-folder-menu-structure.context.js'),
	meta: {
		menuItemAlias: UMB_ELEMENT_MENU_ITEM_ALIAS,
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
		},
	],
};

const workspaceFooterApp: ManifestWorkspaceFooterApp = {
	type: 'workspaceFooterApp',
	kind: 'menuBreadcrumb',
	alias: 'Umb.WorkspaceFooterApp.ElementFolder.Breadcrumb',
	name: 'Element Folder Breadcrumb Workspace Footer App',
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [
	workspace,
	workspaceView,
	workspaceAction,
	menuStructure,
	workspaceFooterApp,
];
