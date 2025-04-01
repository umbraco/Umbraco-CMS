import { UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS } from '@umbraco-cms/backoffice/repository';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.MemberTypePropertyTypeReferenceResponse',
		name: 'Member Type Property Type Reference Response Management Api Data Mapping',
		api: () => import('./member-type-property-type-reference-response.management-api.mapping.js'),
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: 'MemberTypePropertyReferenceResponseModel',
	},
];
