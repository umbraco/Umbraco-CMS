import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const UMB_RELATION_TYPE_WORKSPACE_ALIAS = 'Umb.Workspace.RelationType';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_RELATION_TYPE_WORKSPACE_ALIAS,
		name: 'Relation Type Workspace',
		api: () => import('./relation-type-workspace.context.js'),
		meta: {
			entityType: 'relation-type',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.RelationType.Details',
		name: 'Relation Type Details Workspace View',
		js: () => import('./views/relation-type-detail-workspace-view.element.js'),
		weight: 20,
		meta: {
			label: '#general_details',
			pathname: 'details',
			icon: 'icon-trafic',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_RELATION_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];
