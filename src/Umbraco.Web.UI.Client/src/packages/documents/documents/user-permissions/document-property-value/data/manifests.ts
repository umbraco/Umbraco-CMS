import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE } from '../user-permission.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS } from '@umbraco-cms/backoffice/repository';
import { UmbDocumentPropertyValueUserPermissionFromManagementApiDataMapping } from './from-server.management-api.mapping.js';
import { UmbDocumentPropertyValueUserPermissionToManagementApiDataMapping } from './to-server.management-api.mapping.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.To.DocumentPropertyValuePermissionPresentationModel',
		name: 'Document Property Value Permission To Management Api Data Mapping',
		api: UmbDocumentPropertyValueUserPermissionToManagementApiDataMapping,
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_TYPE,
	},
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.From.DocumentPropertyValuePermissionPresentationModel',
		name: 'Document Property Value Permission From Management Api Data Mapping',
		api: UmbDocumentPropertyValueUserPermissionFromManagementApiDataMapping,
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: 'DocumentPropertyValuePermissionPresentationModel',
	},
];
