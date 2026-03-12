import { UMB_USER_COLLECTION_ALIAS } from '../../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFacetFilter',
		alias: 'Umb.CollectionFacetFilter.UserState',
		name: 'User State Collection Filter',
		weight: 200,
		element: () => import('./user-state-collection-filter.element.js'),
		api: () => import('./user-state-collection-filter.api.js'),
		meta: {
			label: 'Status',
			filterKey: 'userStates',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
];
