import {
	UMB_BLOCK_ENTRY_IS_LIBRARY_ELEMENT_CONDITION_ALIAS,
	UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
} from '../../../conditions/constants.js';
import { UMB_BLOCK_ACTION_TRANSFER_TO_LIBRARY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockAction',
		kind: 'default',
		alias: UMB_BLOCK_ACTION_TRANSFER_TO_LIBRARY_ALIAS,
		name: 'Transfer Block To Library Action',
		weight: 250,
		api: () => import('./transfer-to-library-block.action.js'),
		meta: {
			icon: 'icon-link',
			label: '#blockEditor_transferToLibrary',
		},
		conditions: [
			{
				alias: UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
				match: false,
			},
			{
				alias: UMB_BLOCK_ENTRY_IS_LIBRARY_ELEMENT_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
