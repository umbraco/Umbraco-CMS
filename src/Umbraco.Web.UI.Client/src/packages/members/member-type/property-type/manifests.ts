import { UMB_MEMBER_TYPE_PROPERTY_TYPE_ENTITY_TYPE } from './entity.js';
import { UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS } from '@umbraco-cms/backoffice/repository';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.MemberTypePropertyTypeReferenceResponse',
		name: 'Member Type Property Type Reference Response Management Api Data Mapping',
		api: () => import('./member-type-property-type-reference-response.management-api.mapping.js'),
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: 'MemberTypePropertyTypeReferenceResponseModel',
	},
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.MemberTypePropertyType',
		name: 'Member Type Property Type Entity Item Reference',
		element: () => import('./member-type-property-type-item-ref.element.js'),
		forEntityTypes: [UMB_MEMBER_TYPE_PROPERTY_TYPE_ENTITY_TYPE],
	},
];
