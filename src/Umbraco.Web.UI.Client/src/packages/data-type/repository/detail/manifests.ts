import { UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_DATA_TYPE_DETAIL_STORE_ALIAS } from './constants.js';
import { UmbManagementApiDataTypeDetailDataCacheInvalidationManager } from './server-data-source/data-type-detail.server.cache-invalidation.manager.js';
import { UmbDataTypeDetailStore } from './data-type-detail.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
		name: 'Data Type Detail Repository',
		api: () => import('./data-type-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DATA_TYPE_DETAIL_STORE_ALIAS,
		name: 'Data Type Detail Store',
		api: UmbDataTypeDetailStore,
	},
	{
		name: 'Data Type Detail Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.DataType.Detail',
		type: 'globalContext',
		api: UmbManagementApiDataTypeDetailDataCacheInvalidationManager,
	},
];
