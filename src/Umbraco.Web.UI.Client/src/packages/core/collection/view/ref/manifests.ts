import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionView.Ref',
		matchKind: 'ref',
		matchType: 'collectionView',
		manifest: {
			type: 'collectionView',
			kind: 'ref',
			element: () => import('./ref-collection-view.element.js'),
			weight: 800,
			meta: {
				label: 'List',
				icon: 'icon-list',
				pathName: 'refs',
			},
		},
	},
];
