import { UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
		name: 'Document Reference Repository',
		api: () => import('./document-reference.repository.js'),
	},
	{
		type: 'dataMapper',
		alias: 'Umb.DataMapper.DocumentReferenceResponseModel',
		name: 'Document Reference Response Model to Client Model Data Mapper',
		api: () => import('./document-reference-response-model.mapper.js'),
		identifier: 'DocumentReferenceResponseModel',
	},
];
