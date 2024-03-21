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
		alias: 'Umb.WorkspaceView.RelationType.RelationType',
		name: 'Relation Type Workspace RelationType View',
		js: () => import('./views/relation-type/relation-type-workspace-view-relation-type.element.js'),
		weight: 20,
		meta: {
			label: 'RelationType',
			pathname: 'relation-type',
			icon: 'icon-info',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.RelationType.Relation',
		name: 'Relation Type Workspace Relation View',
		js: () => import('./views/relation/workspace-view-relation-type-relation.element.js'),
		weight: 10,
		meta: {
			label: 'Relation',
			pathname: 'relation',
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
