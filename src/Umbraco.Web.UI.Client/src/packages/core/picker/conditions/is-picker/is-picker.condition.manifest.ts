import { UMB_IS_PICKER_CONDITION_ALIAS } from './constants.js';
import { UmbIsPickerCondition } from './is-picker.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Is Picker Condition',
	alias: UMB_IS_PICKER_CONDITION_ALIAS,
	api: UmbIsPickerCondition,
};
