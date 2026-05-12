import type { UmbMediaValueType } from '../types.js';

export const UMB_UPLOAD_FIELD_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.UploadField' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_UPLOAD_FIELD_PROPERTY_EDITOR_VALUE_TYPE]: UmbMediaValueType | undefined;
	}
}
