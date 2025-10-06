import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.PreviewOption.UrlProvider',
	matchKind: 'urlProvider',
	matchType: 'previewOption',
	manifest: {
		type: 'previewOption',
		kind: 'urlProvider',
		weight: 1000,
		api: () => import('./url-provider.preview-option-action.js'),
		elementName: 'umb-workspace-action-menu-item',
		providerAlias: '',
		meta: {
			icon: '',
			label: '(Missing label in manifest)',
		},
	},
};
