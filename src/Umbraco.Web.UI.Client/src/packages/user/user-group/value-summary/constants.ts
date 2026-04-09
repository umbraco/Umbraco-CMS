import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export const UMB_USER_GROUP_REFERENCES_VALUE_TYPE = 'Umb.ValueType.UserGroup.References' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_USER_GROUP_REFERENCES_VALUE_TYPE]: UmbReferenceByUnique[];
	}
}

export * from './sections/constants.js';
