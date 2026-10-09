import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_BLOCK_ACTION_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.BlockAction.Default',
	matchKind: 'default',
	matchType: 'blockAction',
	manifest: {
		type: 'blockAction',
		kind: 'default',
		weight: 1000,
		element: () => import('./block-action.element.js'),
		meta: {
			icon: '',
			label: '',
		},
	},
};
