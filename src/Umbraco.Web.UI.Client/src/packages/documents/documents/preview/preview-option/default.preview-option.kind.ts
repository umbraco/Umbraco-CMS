import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.PreviewOption.Default',
	matchKind: 'default',
	matchType: 'previewOption',
	manifest: {
		type: 'previewOption',
		kind: 'default',
		weight: 1000,
		api: () => import('./default.preview-option-action.js'),
		elementName: 'umb-workspace-action-menu-item',
		meta: {
			icon: '',
			label: '(Missing label in manifest)',
		},
	},
};
