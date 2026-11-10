import {
	UMB_ENTITY_ACTION_GROUP_CREATE,
	UMB_ENTITY_ACTION_GROUP_IMPORT_EXPORT,
	UMB_ENTITY_ACTION_GROUP_NOTIFICATIONS,
	UMB_ENTITY_ACTION_GROUP_PUBLISHING,
	UMB_ENTITY_ACTION_GROUP_ROUTING,
	UMB_ENTITY_ACTION_GROUP_STRUCTURE,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityActionGroup',
		alias: UMB_ENTITY_ACTION_GROUP_CREATE,
		name: 'Create Entity Action Group',
		weight: 1200,
		meta: { label: '#actionGroups_create' },
	},
	{
		type: 'entityActionGroup',
		alias: UMB_ENTITY_ACTION_GROUP_STRUCTURE,
		name: 'Structure Entity Action Group',
		weight: 800,
		meta: { label: '#actionGroups_structure' },
	},
	{
		type: 'entityActionGroup',
		alias: UMB_ENTITY_ACTION_GROUP_PUBLISHING,
		name: 'Publishing Entity Action Group',
		weight: 600,
		meta: { label: '#actionGroups_publishing' },
	},
	{
		type: 'entityActionGroup',
		alias: UMB_ENTITY_ACTION_GROUP_ROUTING,
		name: 'Routing Entity Action Group',
		weight: 400,
		meta: { label: '#actionGroups_routing' },
	},
	{
		type: 'entityActionGroup',
		alias: UMB_ENTITY_ACTION_GROUP_NOTIFICATIONS,
		name: 'Notifications Entity Action Group',
		weight: 100,
		meta: { label: '#actionGroups_notifications' },
	},
	{
		type: 'entityActionGroup',
		alias: UMB_ENTITY_ACTION_GROUP_IMPORT_EXPORT,
		name: 'Import and Export Entity Action Group',
		weight: 50,
		meta: { label: '#actionGroups_importExport' },
	},
];
