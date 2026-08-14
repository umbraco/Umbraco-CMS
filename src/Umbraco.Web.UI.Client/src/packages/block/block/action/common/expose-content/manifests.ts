import {
	UMB_BLOCK_ENTRY_IS_EXPOSED_CONDITION_ALIAS,
	UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
	UMB_BLOCK_ENTRY_SHOW_CONTENT_EDIT_CONDITION_ALIAS,
} from '../../../conditions/constants.js';
import { UMB_BLOCK_ACTION_EXPOSE_CONTENT_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockAction',
		kind: 'default',
		alias: UMB_BLOCK_ACTION_EXPOSE_CONTENT_ALIAS,
		name: 'Expose Content Block Action',
		weight: 399,
		api: () => import('./expose-content-block.action.js'),
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
		},
		conditions: [
			{ alias: UMB_BLOCK_ENTRY_SHOW_CONTENT_EDIT_CONDITION_ALIAS, match: false },
			{ alias: UMB_BLOCK_ENTRY_IS_EXPOSED_CONDITION_ALIAS, match: false },
			{ alias: UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS, match: false },
		],
	},
];
