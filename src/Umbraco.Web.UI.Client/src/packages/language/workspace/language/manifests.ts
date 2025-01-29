import { UMB_LANGUAGE_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_LANGUAGE_WORKSPACE_ALIAS,
		name: 'Language Workspace',
		api: () => import('./language-workspace.context.js'),
		meta: {
			entityType: 'language',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Language.Details',
		name: 'Language Workspace Details View',
		js: () => import('./views/language-details-workspace-view.element.js'),
		weight: 90,
		meta: {
			label: '#general_details',
			pathname: 'details',
			icon: 'edit',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_LANGUAGE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Language.Save',
		name: 'Save Language Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			look: 'primary',
			color: 'positive',
			label: '#buttons_save',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_LANGUAGE_WORKSPACE_ALIAS,
			},
		],
	},
];
