import { UMB_PROPERTY_ACTION_GROUP_CLIPBOARD, UMB_PROPERTY_ACTION_GROUP_EDIT } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyActionGroup',
		alias: UMB_PROPERTY_ACTION_GROUP_CLIPBOARD,
		name: 'Clipboard Property Action Group',
		weight: 1200,
		meta: { label: '#actionGroups_clipboard' },
	},
	{
		type: 'propertyActionGroup',
		alias: UMB_PROPERTY_ACTION_GROUP_EDIT,
		name: 'Edit Property Action Group',
		weight: 1100,
		meta: { label: '#actionGroups_edit' },
	},
];
