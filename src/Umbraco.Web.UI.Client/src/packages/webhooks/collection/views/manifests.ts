import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_WEBHOOK_TABLE_COLLECTION_VIEW_ALIAS = 'Umb.CollectionView.Webhook.Table';

const tableCollectionView: ManifestCollectionView = {
	type: 'collectionView',
	alias: UMB_WEBHOOK_TABLE_COLLECTION_VIEW_ALIAS,
	name: 'Webhook Table Collection View',
	js: () => import('./table/webhook-table-collection-view.element.js'),
	meta: {
		label: 'Table',
		icon: 'icon-list',
		pathName: 'table',
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'Umb.Collection.Webhook',
		},
	],
};

export const manifests = [tableCollectionView];
