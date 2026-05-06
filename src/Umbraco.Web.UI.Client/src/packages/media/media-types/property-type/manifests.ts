import { UMB_MEDIA_TYPE_PROPERTY_TYPE_ENTITY_TYPE } from './entity.js';
import { UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS } from '@umbraco-cms/backoffice/repository';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.MediaTypePropertyTypeReferenceResponse',
		name: 'Media Type Property Type Reference Response Management Api Data Mapping',
		api: () => import('./media-type-property-type-reference-response.management-api.mapping.js'),
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: 'MediaTypePropertyTypeReferenceResponseModel',
	},
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.MediaTypePropertyType',
		name: 'Media Type Property Type Entity Item Reference',
		element: () => import('./media-type-property-type-item-ref.element.js'),
		forEntityTypes: [UMB_MEDIA_TYPE_PROPERTY_TYPE_ENTITY_TYPE],
	},
];
