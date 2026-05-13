import type { UmbReferenceByUniqueAndType } from '@umbraco-cms/backoffice/models';

export const UMB_CONTENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.MultiNodeTreePicker' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_CONTENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE]: Array<UmbReferenceByUniqueAndType> | undefined;
	}
}
