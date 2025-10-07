import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceActionMenuItem.PreviewOption',
	matchType: 'workspaceActionMenuItem',
	matchKind: 'previewOption',
	manifest: {
		type: 'workspaceActionMenuItem',
		kind: 'previewOption',
		weight: 1000,
		api: () => import('./preview-option.action.js'),
		elementName: 'umb-workspace-action-menu-item',
		forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPreview',
		meta: {
			icon: '',
			label: '(Missing label in manifest)',
			urlProviderAlias: 'umbDocumentUrlProvider',
		},
	},
};
