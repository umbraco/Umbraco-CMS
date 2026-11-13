import { UMB_USER_COLLECTION_USER_STATE_FILTER_ALIAS } from './constants.js';
import { UMB_USER_COLLECTION_ALIAS } from '../../constants.js';
import { UmbUserStateDatalistDataSource } from './user-state-datalist-data-source.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionFilter',
		kind: 'select',
		alias: UMB_USER_COLLECTION_USER_STATE_FILTER_ALIAS,
		name: 'User State Collection Filter',
		forCollections: [UMB_USER_COLLECTION_ALIAS],
		weight: 200,
		meta: {
			label: 'Status',
			multiple: true,
			datalistDataSource: UmbUserStateDatalistDataSource,
		},
	},
];
