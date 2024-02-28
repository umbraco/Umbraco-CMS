import type { ManifestWorkspaceActionMenuItem } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestWorkspaceActionMenuItem> = [
	{
		type: 'workspaceActionMenuItem',
		alias: 'Umb.WorkspaceActionMenuItem.Unpublish',
		name: 'Unpublish',
		meta: {
			workspaceActionAliases: ['Umb.WorkspaceAction.Document.SaveAndPublish'],
		},
	},
];
