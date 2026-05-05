import { UMB_BLOCK_ENTRY_HAS_SETTINGS_CONDITION_ALIAS } from '../../../conditions/constants.js';
import { UMB_BLOCK_ACTION_EDIT_SETTINGS_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockAction',
		kind: 'default',
		alias: UMB_BLOCK_ACTION_EDIT_SETTINGS_ALIAS,
		name: 'Edit Settings Block Action',
		weight: 300,
		api: () => import('./edit-settings-block.action.js'),
		meta: {
			icon: 'icon-settings',
			label: '#general_settings',
		},
		conditions: [{ alias: UMB_BLOCK_ENTRY_HAS_SETTINGS_CONDITION_ALIAS }],
	},
];
