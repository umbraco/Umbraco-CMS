import type { ManifestCollectionView } from '@umbraco-cms/models';

export const manifests: Array<ManifestCollectionView> = [
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.Grid',
		name: 'Grid',
		elementName: 'umb-collection-view-media-grid',
		loader: () => import('./collection-view-media-grid.element'),
		weight: 300,
		meta: {
			label: 'Grid',
			icon: 'umb:grid',
			pathName: 'grid',
		},
		conditions: {
			entityType: 'media',
		},
	},
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.Table',
		name: 'Table',
		elementName: 'umb-collection-view-media-table',
		loader: () => import('./collection-view-media-table.element'),
		weight: 200,
		meta: {
			label: 'Table',
			icon: 'umb:box',
			pathName: 'table',
		},
		conditions: {
			entityType: 'media',
		},
	},
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.Test',
		name: 'Test',
		elementName: 'umb-collection-view-media-test',
		loader: () => import('./collection-view-media-test.element'),
		weight: 100,
		meta: {
			label: 'Test',
			icon: 'umb:newspaper',
			pathName: 'test',
		},
		conditions: {
			entityType: 'media',
		},
	},
];
