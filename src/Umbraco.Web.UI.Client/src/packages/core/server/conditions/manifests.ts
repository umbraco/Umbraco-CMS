import { UMB_IS_PRODUCTION_MODE_CONDITION_ALIAS } from './constants.js';
import { UmbIsProductionModeCondition } from './is-production-mode.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Server Is Production Mode Condition',
		alias: UMB_IS_PRODUCTION_MODE_CONDITION_ALIAS,
		api: UmbIsProductionModeCondition,
	},
];
