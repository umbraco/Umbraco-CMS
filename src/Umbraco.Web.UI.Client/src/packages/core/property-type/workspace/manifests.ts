import { UMB_PROPERTY_TYPE_WORKSPACE_ALIAS } from './constants.js';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'workspace',
		kind: 'routable',
		name: 'Block Workspace',
		alias: UMB_PROPERTY_TYPE_WORKSPACE_ALIAS,
		api: () => import('./property-type-workspace.context.js'),
		meta: {
			entityType: 'property-type',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.PropertyType.Settings',
		name: 'Block Workspace Content View',
		js: () => import('./views/settings/property-workspace-view-settings.element.js'),
		weight: 1000,
		meta: {
			label: '#general_content',
			pathname: 'content',
			icon: 'icon-document',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_PROPERTY_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.PropertyType.Submit',
		name: 'Submit Property Type Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#general_submit',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				oneOf: [UMB_PROPERTY_TYPE_WORKSPACE_ALIAS],
			},
		],
	},
];
