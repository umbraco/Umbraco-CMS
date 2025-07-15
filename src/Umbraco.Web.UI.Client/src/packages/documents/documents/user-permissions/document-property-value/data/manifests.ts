import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE } from '../user-permission.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS } from '@umbraco-cms/backoffice/repository';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.To.DocumentPropertyValuePermissionPresentationModel',
		name: 'Document Property Value Permission To Management Api Data Mapping',
		api: () => import('./to-server.management-api.mapping.js'),
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE,
	},
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.From.DocumentPropertyValuePermissionPresentationModel',
		name: 'Document Property Value Permission From Management Api Data Mapping',
		api: () => import('./from-server.management-api.mapping.js'),
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: 'DocumentPropertyValuePermissionPresentationModel',
	},
];
