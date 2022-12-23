import type { ManifestCollectionBulkAction } from '@umbraco-cms/models';

export const manifests: Array<ManifestCollectionBulkAction> = [
	{
		type: 'collectionBulkAction',
		alias: 'Umb.CollectionBulkAction.Test',
		name: 'Test',
		elementName: 'umb-collection-bulk-action-media-test',
		loader: () => import('./collection-bulk-action-media-test.element'),
		weight: 600,
		meta: {
			label: 'Test',
			entityType: 'media',
		},
	},
];
