import type { ManifestWorkspaces, ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';

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
		js: () => import('./views/details/relation-type-details-workspace-view.element.js'),
		weight: 20,
		meta: {
			label: 'Details',
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

export const manifests = [workspace, ...workspaceViews];
