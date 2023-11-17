import { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestCollectionView> = [
	{
		type: 'collectionView',
		alias: 'Umb.CollectionView.Document.Table',
		name: 'Document Table Collection View',
		js: () => import('./views/table/document-table-collection-view.element.js'),
		weight: 200,
		conditions: [{
			alias: 'Umb.Condition.WorkspaceEntityType',
			match: 'document',
		}],
		meta: {
			label: 'Table',
			icon: 'icon-box',
			pathName: 'table',
		},
	},
];
