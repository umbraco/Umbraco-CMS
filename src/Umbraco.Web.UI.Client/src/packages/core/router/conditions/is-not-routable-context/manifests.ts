import { UMB_IS_NOT_ROUTABLE_CONTEXT_CONDITION_ALIAS } from './constants.js';
import { UmbIsNotRoutableContextCondition } from './index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Is Not in a Routable Context Condition',
		alias: UMB_IS_NOT_ROUTABLE_CONTEXT_CONDITION_ALIAS,
		api: UmbIsNotRoutableContextCondition,
	},
];
