import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_COLLECTION_ALIAS = 'document';

export const manifests: Array<ManifestTypes> = [
	// TODO: temp registration, missing collection repository
	{
		type: 'collection',
		kind: 'default',
		alias: 'Umb.Collection.Document',
		name: 'Document Collection',
	},
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.Document.Table',
		name: 'Document Table Collection View',
		js: () => import('./views/table/document-table-collection-view.element.js'),
		weight: 200,
		meta: {
			label: 'Table',
			icon: 'icon-box',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_COLLECTION_ALIAS,
			},
		],
	},
];
