import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_TABLE_COLLECTION_VIEW_ALIAS = 'Umb.CollectionView.Dictionary.Table';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_DICTIONARY_TABLE_COLLECTION_VIEW_ALIAS,
	name: 'Dictionary Table Collection View',
	js: () => import('./table/dictionary-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'icon-list',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Dictionary',
		},
	],
};

export const manifests = [tableCollectionView];
