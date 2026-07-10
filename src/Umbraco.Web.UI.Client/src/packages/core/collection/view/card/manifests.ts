import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbCardCollectionViewElement } from './card-collection-view.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionView.Card',
		matchKind: 'card',
		matchType: 'collectionView',
		manifest: {
			type: 'collectionView',
			kind: 'card',
			element: UmbCardCollectionViewElement,
			weight: 800,
			meta: {
				label: 'Cards',
				icon: 'icon-grid',
				pathName: 'cards',
			},
		},
	},
];
