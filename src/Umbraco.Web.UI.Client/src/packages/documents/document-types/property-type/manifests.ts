import { UMB_DOCUMENT_TYPE_PROPERTY_TYPE_ENTITY_TYPE } from './entity.js';
import { UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS } from '@umbraco-cms/backoffice/repository';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dataSourceDataMapping',
		alias: 'Umb.DataSourceDataMapping.ManagementApi.DocumentTypePropertyTypeReferenceResponse',
		name: 'Document Type Property Type Reference Response Management Api Data Mapping',
		api: () => import('./document-type-property-type-reference-response.management-api.mapping.js'),
		forDataSource: UMB_MANAGEMENT_API_DATA_SOURCE_ALIAS,
		forDataModel: 'DocumentTypePropertyReferenceResponseModel',
	},
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.DocumentTypePropertyType',
		name: 'Document Type Property Type Entity Item Reference',
		element: () => import('./document-type-property-type-item-ref.element.js'),
		forEntityTypes: [UMB_DOCUMENT_TYPE_PROPERTY_TYPE_ENTITY_TYPE],
	},
];
