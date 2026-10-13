import { UmbTableCollectionViewElement } from './table-collection-view.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionView.Table',
		matchKind: 'table',
		matchType: 'collectionView',
		manifest: {
			type: 'collectionView',
			kind: 'table',
			element: UmbTableCollectionViewElement,
			weight: 1000,
			meta: {
				label: 'Table',
				icon: 'icon-table',
				pathName: 'table',
			},
		},
	},
];
