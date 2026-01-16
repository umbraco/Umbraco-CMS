import {
	UmbAllowElementRecycleBinCurrentUserCondition,
	UMB_CURRENT_USER_ALLOW_ELEMENT_RECYCLE_BIN_CONDITION_ALIAS,
} from './allow-element-recycle-bin.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Allow Element Recycle Bin Current User Condition',
		alias: UMB_CURRENT_USER_ALLOW_ELEMENT_RECYCLE_BIN_CONDITION_ALIAS,
		api: UmbAllowElementRecycleBinCurrentUserCondition,
	},
];
