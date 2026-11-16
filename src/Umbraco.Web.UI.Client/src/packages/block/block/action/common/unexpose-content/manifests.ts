import {
	UMB_BLOCK_ENTRY_IS_EXPOSED_CONDITION_ALIAS,
	UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
	UMB_BLOCK_ENTRY_VARIES_BY_VARIANT_CONDITION_ALIAS,
} from '../../../conditions/constants.js';
import { UMB_BLOCK_ACTION_UNEXPOSE_CONTENT_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockAction',
		kind: 'default',
		alias: UMB_BLOCK_ACTION_UNEXPOSE_CONTENT_ALIAS,
		name: 'Unexpose Content Block Action',
		weight: 200,
		api: () => import('./unexpose-content-block.action.js'),
		meta: {
			icon: 'icon-remove',
			label: '#blockEditor_removeFromLanguage',
		},
		conditions: [
			{ alias: UMB_BLOCK_ENTRY_VARIES_BY_VARIANT_CONDITION_ALIAS },
			{ alias: UMB_BLOCK_ENTRY_IS_EXPOSED_CONDITION_ALIAS },
			{ alias: UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS, match: false },
		],
	},
];
