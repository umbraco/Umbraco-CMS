import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		name: 'Example Manipulate Document Property Value Write Permissions Workspace Action',
		alias: 'example.workspaceAction.manipulate.write',
		weight: 1000,
		api: () => import('./manipulate-property-write-permissions-action.js'),
		meta: {
			label: 'Toggle write richTextEditor',
			look: 'primary',
			color: 'danger',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		name: 'Example Manipulate Document Property Value Readonly Permissions Workspace Action',
		alias: 'example.workspaceAction.manipulate.readonly',
		weight: 1000,
		api: () => import('./manipulate-property-readonly-permissions-action.js'),
		meta: {
			label: 'Toggle readonly richTextEditor',
			look: 'primary',
			color: 'danger',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		name: 'Example Manipulate Document Property Value View Permissions Workspace Action',
		alias: 'example.workspaceAction.manipulate.view',
		weight: 1000,
		api: () => import('./manipulate-property-view-permissions-action.js'),
		meta: {
			label: 'Toggle view richTextEditor',
			look: 'primary',
			color: 'danger',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
