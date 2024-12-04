import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.PropertyAction.Default',
	matchKind: 'default',
	matchType: 'propertyAction',
	manifest: {
		type: 'propertyAction',
		kind: 'default',
		weight: 1000,
		element: () => import('./property-action.element.js'),
		meta: {
			icon: 'icon-bug',
			label: '(Missing label in manifest)',
		},
	},
};
