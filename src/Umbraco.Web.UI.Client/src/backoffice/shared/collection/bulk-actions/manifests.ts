import type { ManifestCollectionBulkAction } from '@umbraco-cms/models';

export const manifests: Array<ManifestCollectionBulkAction> = [
	{
		type: 'collectionBulkAction',
		alias: 'Umb.CollectionBulkAction.Delete',
		name: 'Delete',
		elementName: 'umb-collection-bulk-action-media-delete',
		loader: () => import('./collection-bulk-action-media-delete.element'),
		weight: 200,
		meta: {
			label: 'Delete',
			entityType: 'media',
		},
	},
];
