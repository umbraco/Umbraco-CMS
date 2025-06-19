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
			label: 'Toggle write RTE',
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
			label: 'Toggle view RTE',
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
		name: 'Example Manipulate Document Property Value Write Complex Permissions Workspace Action',
		alias: 'example.workspaceAction.manipulate.writeComplex',
		weight: 1000,
		api: () => import('./manipulate-property-write-complex-permissions-action.js'),
		meta: {
			label: 'Toggle composed write RTE',
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
