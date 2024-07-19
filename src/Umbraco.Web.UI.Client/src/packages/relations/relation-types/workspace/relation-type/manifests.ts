import type {
	ManifestTypes,
	ManifestWorkspaces,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspaces = {
	type: 'workspace',
	kind: 'routable',
	alias: 'Umb.Workspace.RelationType',
	name: 'Relation Type Workspace',
	api: () => import('./relation-type-workspace.context.js'),
	meta: {
		entityType: 'relation-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
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
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews];
