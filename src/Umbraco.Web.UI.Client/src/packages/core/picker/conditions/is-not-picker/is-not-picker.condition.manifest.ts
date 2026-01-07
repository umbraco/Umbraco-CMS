import { UMB_IS_NOT_PICKER_CONDITION_ALIAS } from './constants.js';
import { UmbIsNotPickerCondition } from './is-not-picker.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Is Not Picker Condition',
	alias: UMB_IS_NOT_PICKER_CONDITION_ALIAS,
	api: UmbIsNotPickerCondition,
};
