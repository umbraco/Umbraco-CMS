import type { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export const UMB_USER_STATE_VALUE_TYPE = 'Umb.ValueType.User.State' as const;
export const UMB_USER_LAST_LOGIN_VALUE_TYPE = 'Umb.ValueType.User.LastLogin' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_USER_STATE_VALUE_TYPE]: UserStateModel;
		[UMB_USER_LAST_LOGIN_VALUE_TYPE]: string;
	}
}
