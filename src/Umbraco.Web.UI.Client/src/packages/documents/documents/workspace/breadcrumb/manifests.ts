import type { ManifestWorkspaceFooterApp } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestWorkspaceFooterApp> = [
	{
		type: 'workspaceFooterApp',
		kind: 'variantBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Document.Breadcrumb',
		name: 'Document Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Document',
			},
		],
	},
];
