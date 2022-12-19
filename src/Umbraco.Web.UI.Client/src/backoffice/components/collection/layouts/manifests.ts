import type { ManifestCollectionLayout } from '@umbraco-cms/models';

export const manifests: Array<ManifestCollectionLayout> = [
	{
		type: 'collectionLayout',
		alias: 'Umb.CollectionLayout.Grid',
		name: 'Grid',
		elementName: 'umb-collection-layout-grid',
		loader: () => import('./collection-layout-grid.element'),
		weight: 100,
		meta: {
			label: 'Grid',
			icon: 'umb:grid',
			entityType: 'media',
			pathName: 'grid',
		},
	},
];
