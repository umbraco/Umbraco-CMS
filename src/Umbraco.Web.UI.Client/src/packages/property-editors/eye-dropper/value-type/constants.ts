export const UMB_EYE_DROPPER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.ColorPicker.EyeDropper' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_EYE_DROPPER_PROPERTY_EDITOR_VALUE_TYPE]: string | undefined;
	}
}
