import type { ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Collection',
		name: 'Workspace Collection View',
		element: () => import('./workspace-view-collection.element.js'),
		weight: 300,
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-grid',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				oneOf: ['Umb.Workspace.Document', 'Umb.Workspace.Media'],
			},
			{
				alias: 'Umb.Condition.WorkspaceHasCollection',
			},
		],
	},
];
