import { UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS } from './constants.js';
import { UMB_MANAGEMENT_API_DATA_SOURCE_IDENTIFIER } from '@umbraco-cms/backoffice/repository';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
		name: 'Document Reference Repository',
		api: () => import('./document-reference.repository.js'),
	},
	{
		type: 'dataMapping',
		alias: 'Umb.DataMapping.ManagementApi.DocumentReferenceResponse',
		name: 'Document Reference Response Management Api Data Mapping',
		api: () => import('./document-reference-response.management-api.mapping.js'),
		dataSourceIdentifier: UMB_MANAGEMENT_API_DATA_SOURCE_IDENTIFIER,
		dataModelIdentifier: 'DocumentReferenceResponseModel',
	},
];
