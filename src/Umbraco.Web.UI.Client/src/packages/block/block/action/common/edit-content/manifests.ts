import { UMB_BLOCK_ENTRY_SHOW_CONTENT_EDIT_CONDITION_ALIAS } from '../../../conditions/constants.js';
import { UMB_BLOCK_ACTION_EDIT_CONTENT_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'blockAction',
		kind: 'default',
		alias: UMB_BLOCK_ACTION_EDIT_CONTENT_ALIAS,
		name: 'Edit Content Block Action',
		weight: 400,
		api: () => import('./edit-content-block.action.js'),
		meta: {
			icon: 'icon-edit',
			label: '#general_edit',
		},
		conditions: [{ alias: UMB_BLOCK_ENTRY_SHOW_CONTENT_EDIT_CONDITION_ALIAS }],
	},
];
