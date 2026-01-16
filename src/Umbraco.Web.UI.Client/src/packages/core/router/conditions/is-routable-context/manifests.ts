import { UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS } from './constants.js';
import { UmbIsRoutableContextCondition } from './index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Is in a Routable Context Condition',
		alias: UMB_IS_ROUTABLE_CONTEXT_CONDITION_ALIAS,
		api: UmbIsRoutableContextCondition,
	},
];
