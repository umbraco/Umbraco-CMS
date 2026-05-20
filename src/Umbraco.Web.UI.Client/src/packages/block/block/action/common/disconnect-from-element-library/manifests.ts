import {
	UMB_BLOCK_ENTRY_IS_LIBRARY_ELEMENT_CONDITION_ALIAS,
	UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
} from '../../../conditions/constants.js';
import { UMB_BLOCK_ACTION_DISCONNECT_FROM_ELEMENT_LIBRARY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockAction',
		kind: 'default',
		alias: UMB_BLOCK_ACTION_DISCONNECT_FROM_ELEMENT_LIBRARY_ALIAS,
		name: 'Disconnect Block From Element Library Action',
		weight: 250,
		api: () => import('./disconnect-from-element-library-block.action.js'),
		meta: {
			icon: 'icon-unlink',
			label: '#blockEditor_disconnectFromElementLibrary',
		},
		conditions: [
			{
				alias: UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
				match: false,
			},
			{
				alias: UMB_BLOCK_ENTRY_IS_LIBRARY_ELEMENT_CONDITION_ALIAS,
				match: true,
			},
		],
	},
];
