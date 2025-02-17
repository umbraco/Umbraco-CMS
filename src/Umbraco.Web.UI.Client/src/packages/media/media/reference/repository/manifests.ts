import { UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS } from './constants.js';
import { UMB_MANAGEMENT_API_DATA_SOURCE_IDENTIFIER } from '@umbraco-cms/backoffice/repository';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS,
		name: 'Media Reference Repository',
		api: () => import('./media-reference.repository.js'),
	},
	{
		type: 'dataMapping',
		alias: 'Umb.DataMapping.ManagementApi.MediaReferenceResponse',
		name: 'Media Reference Response Management Api Data Mapping',
		api: () => import('./media-reference-response.management-api.mapping.js'),
		dataSourceIdentifier: UMB_MANAGEMENT_API_DATA_SOURCE_IDENTIFIER,
		dataModelIdentifier: 'MediaReferenceResponseModel',
	},
];
