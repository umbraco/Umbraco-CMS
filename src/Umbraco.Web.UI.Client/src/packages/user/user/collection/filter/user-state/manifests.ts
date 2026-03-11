import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_USER_COLLECTION_ALIAS } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		alias: 'Umb.CollectionFilter.UserState',
		name: 'User State Collection Filter',
		weight: 200,
		element: () => import('./user-state-collection-filter.element.js'),
		api: () => import('./user-state-collection-filter.api.js'),
		meta: {
			label: 'Status',
			filterKey: 'state',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
];
