import { UMB_BLOCK_ACTION_DELETE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockAction',
		kind: 'default',
		alias: UMB_BLOCK_ACTION_DELETE_ALIAS,
		name: 'Delete Block Action',
		weight: 100,
		api: () => import('./delete-block.action.js'),
		meta: {
			icon: 'icon-remove',
			label: '#general_delete',
		},
	},
];
