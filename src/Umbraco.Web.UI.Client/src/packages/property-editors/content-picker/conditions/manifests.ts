import { UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS } from './constants.js';
import { UmbContentPickerSourceTypeCondition } from './content-picker-source-type.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Content Picker Source Type Condition',
		alias: UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS,
		api: UmbContentPickerSourceTypeCondition,
	},
];
