import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'workspaceFooterApp',
		alias: 'Umb.WorkspaceFooterApp.Document.Breadcrumb',
		name: 'Document Breadcrumb Workspace Footer App',
		weight: 10,
		element: () => import('./workspace-breadcrumb.element.js'),
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
