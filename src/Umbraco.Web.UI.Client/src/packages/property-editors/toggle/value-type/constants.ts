import type { UmbTogglePropertyEditorUiValue } from '../types.js';

export const UMB_TOGGLE_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.TrueFalse' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_TOGGLE_PROPERTY_EDITOR_VALUE_TYPE]: UmbTogglePropertyEditorUiValue;
	}
}
