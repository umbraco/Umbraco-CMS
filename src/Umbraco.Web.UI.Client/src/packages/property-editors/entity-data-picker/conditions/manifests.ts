import { UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS } from './constants.js';
import { UmbEntityDataPickerSupportsTextFilterCondition } from './entity-data-picker-supports-text-filter.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifests: Array<ManifestCondition> = [
	{
		type: 'condition',
		name: 'Entity Data Picker Supports Text Filter Condition',
		alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS,
		api: UmbEntityDataPickerSupportsTextFilterCondition,
	},
];
