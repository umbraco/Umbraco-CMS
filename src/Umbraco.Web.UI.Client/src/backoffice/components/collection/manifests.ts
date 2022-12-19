import type { ManifestCollectionBulkAction } from '@umbraco-cms/models';

export const manifests: Array<ManifestCollectionBulkAction> = [
	{
		type: 'collectionBulkAction',
		alias: 'Umb.CollectionBulkAction.Test',
		name: 'Test',
		elementName: 'umb-collection-bulk-action',
		loader: () => import('./collection-bulk-action.element'),
		weight: 600,
		meta: {
			label: 'Test',
			entityType: 'media',
		},
	},
];
