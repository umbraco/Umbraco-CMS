import { UMB_USER_COLLECTION_USER_GROUP_FILTER_ALIAS } from './constants.js';
import { UMB_USER_COLLECTION_ALIAS } from '../../constants.js';
import { UmbUserGroupDatalistDataSource } from './user-group-datalist-data-source.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		kind: 'select',
		alias: UMB_USER_COLLECTION_USER_GROUP_FILTER_ALIAS,
		name: 'User Group Collection Filter',
		forCollections: [UMB_USER_COLLECTION_ALIAS],
		weight: 100,
		meta: {
			label: 'Groups',
			multiple: true,
			datalistDataSource: UmbUserGroupDatalistDataSource,
		},
	},
	{
		type: 'filterActiveValue',
		kind: 'default',
		alias: 'Umb.FilterActiveValue.UserCollection.UserGroup',
		name: 'User Group Collection Filter Active Value',
		forFilters: [UMB_USER_COLLECTION_USER_GROUP_FILTER_ALIAS],
		valueResolver: () => import('./user-group-filter-value.resolver.js'),
	},
];
