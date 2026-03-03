import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from './constants.js';
import { UmbIsServerProductionModeCondition } from './is-production-mode.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Server Production Mode Condition',
		alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
		api: UmbIsServerProductionModeCondition,
	},
];
