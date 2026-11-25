import { UMB_MEDIA_USER_START_NODE_ACCESS_CONDITION_ALIAS } from './constants.js';
import { UmbMediaUserStartNodeAccessCondition } from './media-user-start-node-access.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Media User Start Node Access Condition',
		alias: UMB_MEDIA_USER_START_NODE_ACCESS_CONDITION_ALIAS,
		api: UmbMediaUserStartNodeAccessCondition,
	},
];
