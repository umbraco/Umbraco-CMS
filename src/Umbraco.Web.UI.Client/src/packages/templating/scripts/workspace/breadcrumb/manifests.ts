import type { ManifestWorkspaceFooterApp } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestWorkspaceFooterApp> = [
	{
		type: 'workspaceFooterApp',
		kind: 'breadcrumb',
		alias: 'Umb.WorkspaceFooterApp.Script.Breadcrumb',
		name: 'Script Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Script',
			},
		],
	},
];
