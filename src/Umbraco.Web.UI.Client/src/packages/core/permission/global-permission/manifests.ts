import { UMB_GLOBAL_PERMISSION_MANIFEST_SOURCE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: UMB_GLOBAL_PERMISSION_MANIFEST_SOURCE_ALIAS,
		name: 'Global Permission Manifest Source',
		api: () => import('./global-permission-manifest.source.js'),
	},
];
