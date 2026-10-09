import { UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS } from '../../../conditions/constants.js';
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
		conditions: [
			{
				alias: UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
