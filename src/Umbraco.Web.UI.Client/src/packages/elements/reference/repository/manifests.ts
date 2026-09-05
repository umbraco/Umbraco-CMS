import { UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS } from './constants.js';
import { UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS } from '@umbraco-cms/backoffice/repository';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS,
		name: 'Element Reference Repository',
		api: () => import('./element-reference.repository.js'),
	},
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.ElementReferenceResponse',
		name: 'Element Reference Response Management Api Data Mapping',
		api: () => import('./element-reference-response.management-api.mapping.js'),
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: 'ElementReferenceResponseModel',
	},
];
