import { UMB_SEGMENT_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_SEGMENT_COLLECTION_REPOSITORY_ALIAS,
		name: 'Segment Collection Repository',
		api: () => import('./segment-collection.repository.js'),
	},
];
