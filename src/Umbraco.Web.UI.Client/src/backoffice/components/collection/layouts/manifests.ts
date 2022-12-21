import type { ManifestCollectionView } from '@umbraco-cms/models';

export const manifests: Array<ManifestCollectionView> = [
	{
		type: 'collectionLayout',
		alias: 'Umb.CollectionLayout.Grid',
		name: 'Grid',
		elementName: 'umb-collection-layout-media-grid',
		loader: () => import('./collection-layout-media-grid.element'),
		weight: 300,
		meta: {
			label: 'Grid',
			icon: 'umb:grid',
			entityType: 'media',
			pathName: 'grid',
		},
	},
	{
		type: 'collectionLayout',
		alias: 'Umb.CollectionLayout.Table',
		name: 'Table',
		elementName: 'umb-collection-layout-media-table',
		loader: () => import('./collection-layout-media-table.element'),
		weight: 200,
		meta: {
			label: 'Table',
			icon: 'umb:box',
			entityType: 'media',
			pathName: 'table',
		},
	},
	{
		type: 'collectionLayout',
		alias: 'Umb.CollectionLayout.Test',
		name: 'Test',
		elementName: 'umb-collection-layout-media-test',
		loader: () => import('./collection-layout-media-test.element'),
		weight: 100,
		meta: {
			label: 'Test',
			icon: 'umb:newspaper',
			entityType: 'media',
			pathName: 'test',
		},
	},
];
