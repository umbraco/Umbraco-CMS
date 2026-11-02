import {
	UMB_WORKSPACE_ACTION_MENU_ITEM_GROUP_DEPLOYMENT,
	UMB_WORKSPACE_ACTION_MENU_ITEM_GROUP_PUBLISHING,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		// Empty in core, for packages that deploy or synchronize entities between environments.
		type: 'workspaceActionMenuItemGroup',
		alias: UMB_WORKSPACE_ACTION_MENU_ITEM_GROUP_DEPLOYMENT,
		name: 'Deployment Workspace Action Menu Item Group',
		weight: 200,
		meta: { label: '#actionGroups_deployment' },
	},
	{
		type: 'workspaceActionMenuItemGroup',
		alias: UMB_WORKSPACE_ACTION_MENU_ITEM_GROUP_PUBLISHING,
		name: 'Publishing Workspace Action Menu Item Group',
		weight: 100,
		meta: { label: '#actionGroups_publishing' },
	},
];
