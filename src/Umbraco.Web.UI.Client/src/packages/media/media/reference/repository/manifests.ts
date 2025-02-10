import { UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS,
		name: 'Media Reference Repository',
		api: () => import('./media-reference.repository.js'),
	},
	{
		type: '$typeDataMapper',
		alias: 'Umb.$typeDataMapper.MediaReferenceResponseModel',
		name: 'Media Reference Response Model to Client Model Data Mapper',
		api: () => import('./media-reference-response-model.mapper.js'),
		from$type: 'MediaReferenceResponseModel',
	},
];
