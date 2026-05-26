import type { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export const UMB_USER_STATE_VALUE_TYPE = 'Umb.ValueType.User.State' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_USER_STATE_VALUE_TYPE]: UserStateModel;
	}
}
