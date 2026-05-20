import {
	UMB_BLOCK_ENTRY_IS_LIBRARY_ELEMENT_CONDITION_ALIAS,
	UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
} from '../../../conditions/constants.js';
import { UMB_BLOCK_ACTION_TRANSFER_TO_ELEMENT_LIBRARY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockAction',
		kind: 'default',
		alias: UMB_BLOCK_ACTION_TRANSFER_TO_ELEMENT_LIBRARY_ALIAS,
		name: 'Transfer Block To Element Library Action',
		weight: 250,
		api: () => import('./transfer-to-element-library-block.action.js'),
		meta: {
			icon: 'icon-link',
			label: '#blockEditor_transferToElementLibrary',
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
