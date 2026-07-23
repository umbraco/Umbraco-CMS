import { UMB_ELEMENT_MENU_ITEM_ALIAS } from '../menu/constants.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_UPDATE,
} from '../user-permissions/constants.js';
import { UMB_ELEMENT_WORKSPACE_ALIAS } from './constants.js';
import { manifests as elementRoot } from './element-root/manifests.js';
import { UmbSubmitWorkspaceAction, UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import type { ManifestWorkspaceContextMenuStructureKind } from '@umbraco-cms/backoffice/menu';
import type {
	ManifestWorkspaceAction,
	ManifestWorkspaceFooterApp,
	ManifestWorkspaceRoutableKind,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/workspace';

const workspace: ManifestWorkspaceRoutableKind = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_ELEMENT_WORKSPACE_ALIAS,
	name: 'Element Workspace',
	api: () => import('./element-workspace.context.js'),
	meta: {
		entityType: UMB_ELEMENT_ENTITY_TYPE,
	},
};

const menuStructure: ManifestWorkspaceContextMenuStructureKind = {
	type: 'workspaceContext',
	kind: 'menuStructure',
	name: 'Element Menu Structure Workspace Context',
	alias: 'Umb.Context.Element.Menu.Structure',
	api: () => import('./element-menu-structure.context.js'),
	meta: {
		menuItemAlias: UMB_ELEMENT_MENU_ITEM_ALIAS,
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: UMB_ELEMENT_WORKSPACE_ALIAS,
		},
		{
			alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
		},
	],
};

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Element.Save',
		name: 'Save Element Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'secondary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_ELEMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_UPDATE],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		kind: 'contentEditor',
		alias: 'Umb.WorkspaceView.Element.Edit',
		name: 'Element Workspace Edit View',
		weight: 200,
		meta: {
			label: '#general_content',
			pathname: 'content',
			icon: 'document',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_ELEMENT_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Element.Info',
		name: 'Element Workspace Info View',
		element: () => import('./views/info/element-workspace-view-info.element.js'),
		weight: 100,
		meta: {
			label: '#general_info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_ELEMENT_WORKSPACE_ALIAS,
			},
		],
	},
];

const workspaceFooterApp: ManifestWorkspaceFooterApp = {
	type: 'workspaceFooterApp',
	kind: 'variantMenuBreadcrumb',
	alias: 'Umb.WorkspaceFooterApp.Element.Breadcrumb',
	name: 'Element Breadcrumb Workspace Footer App',
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: UMB_ELEMENT_WORKSPACE_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [
	...elementRoot,
	menuStructure,
	workspace,
	...workspaceActions,
	...workspaceViews,
	workspaceFooterApp,
];
