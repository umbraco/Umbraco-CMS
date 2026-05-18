import { UMB_MEMBER_REFERENCE_REPOSITORY_ALIAS } from './constants.js';
import { UmbMemberReferenceResponseManagementApiDataMapping } from './member-reference-response.management-api.mapping.js';
import { UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS } from '@umbraco-cms/backoffice/repository';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_REFERENCE_REPOSITORY_ALIAS,
		name: 'Member Reference Repository',
		api: () => import('./member-reference.repository.js'),
	},
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.MemberReferenceResponse',
		name: 'Member Reference Response Management Api Data Mapping',
		api: UmbMemberReferenceResponseManagementApiDataMapping,
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: 'MemberReferenceResponseModel',
	},
];
