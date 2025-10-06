import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceActionMenuItem.UrlProvider',
	matchType: 'workspaceActionMenuItem',
	matchKind: 'urlProvider',
	manifest: {
		type: 'workspaceActionMenuItem',
		kind: 'urlProvider',
		weight: 1000,
		api: () => import('./url-provider.action.js'),
		elementName: 'umb-workspace-action-menu-item',
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPreview',
		urlProviderAlias: 'umbDocumentUrlProvider',
		meta: {
			icon: '',
			label: '(Missing label in manifest)',
		},
	},
};
