import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_EXTENSION_TABLE_COLLECTION_VIEW_ALIAS = 'Umb.CollectionView.Extension.Table';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_EXTENSION_TABLE_COLLECTION_VIEW_ALIAS,
	name: 'Extension Table Collection View',
	js: () => import('./table/extension-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'icon-list',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Extension',
		},
	],
};

export const manifests = [tableCollectionView];
