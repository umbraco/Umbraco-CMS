import { UMB_WORKSPACE_ACTION_MENU_ITEM_GROUP_PUBLISHING } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceActionMenuItemGroup',
		alias: UMB_WORKSPACE_ACTION_MENU_ITEM_GROUP_PUBLISHING,
		name: 'Publishing Workspace Action Menu Item Group',
		weight: 100,
		meta: { label: '#actionGroups_publishing' },
	},
];
