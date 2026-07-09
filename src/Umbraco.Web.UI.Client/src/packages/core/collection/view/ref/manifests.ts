import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbRefCollectionViewElement } from './ref-collection-view.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionView.Ref',
		matchKind: 'ref',
		matchType: 'collectionView',
		manifest: {
			type: 'collectionView',
			kind: 'ref',
			element: UmbRefCollectionViewElement,
			weight: 800,
			meta: {
				label: 'List',
				icon: 'icon-list',
				pathname: 'refs',
			},
		},
	},
];
