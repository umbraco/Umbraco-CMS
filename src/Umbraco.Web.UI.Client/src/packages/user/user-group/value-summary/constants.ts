import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export const UMB_USER_GROUPS_VALUE_TYPE = 'Umb.ValueType.User.UserGroups' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_USER_GROUPS_VALUE_TYPE]: UmbReferenceByUnique[];
	}
}
