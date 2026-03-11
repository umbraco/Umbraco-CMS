import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		alias: 'Umb.CollectionFilter.Extension.Type',
		name: 'Extension Type Collection Filter',
		element: () => import('./extension-collection-filter.element.js'),
		api: () => import('./extension-collection-filter.api.js'),
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Extension',
			},
		],
	},
];
