import { UMB_IMAGING_REPOSITORY_ALIAS, UMB_IMAGING_STORE_ALIAS } from './constants.js';
import { UmbImagingStore } from './imaging.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_IMAGING_REPOSITORY_ALIAS,
		name: 'Imaging Repository',
		api: () => import('./imaging.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_IMAGING_STORE_ALIAS,
		name: 'Imaging Store',
		api: UmbImagingStore,
	},
];
