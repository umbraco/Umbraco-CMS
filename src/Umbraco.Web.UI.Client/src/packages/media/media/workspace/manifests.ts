import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspaces,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION } from '@umbraco-cms/backoffice/content';

const workspace: ManifestWorkspaces = {
	type: 'workspace',
	kind: 'routable',
	alias: 'Umb.Workspace.Media',
	name: 'Media Workspace',
	api: () => import('./media-workspace.context.js'),
	meta: {
		entityType: 'media',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.Media.Collection',
		name: 'Media Workspace Collection View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-grid',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
			{
				alias: 'Umb.Condition.WorkspaceHasCollection',
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'contentEditor',
		alias: 'Umb.WorkspaceView.Media.Edit',
		name: 'Media Workspace Edit View',
		weight: 200,
		meta: {
			label: '#general_details',
			pathname: 'media',
			icon: 'icon-picture',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
			{
				alias: UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Media.Info',
		name: 'Media Workspace Info View',
		element: () => import('./views/info/media-workspace-view-info.element.js'),
		weight: 100,
		meta: {
			label: '#general_info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Media.Save',
		name: 'Save Media Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews, ...workspaceActions];
