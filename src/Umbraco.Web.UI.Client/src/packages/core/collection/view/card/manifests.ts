import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionView.Card',
		matchKind: 'card',
		matchType: 'collectionView',
		manifest: {
			type: 'collectionView',
			kind: 'card',
			element: () => import('./card-collection-view.element.js'),
			weight: 800,
			meta: {
				label: 'Cards',
				icon: 'icon-grid',
				pathName: 'cards',
			},
		},
	},
];
