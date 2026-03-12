import { UMB_USER_COLLECTION_ALIAS } from '../../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFacetFilter',
		alias: 'Umb.CollectionFacetFilter.UserGroup',
		name: 'User Group Collection Filter',
		weight: 100,
		element: () => import('./user-group-collection-filter.element.js'),
		api: () => import('./user-group-collection-filter.api.js'),
		meta: {
			label: 'Groups',
			filterKey: 'userGroupIds',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
];
