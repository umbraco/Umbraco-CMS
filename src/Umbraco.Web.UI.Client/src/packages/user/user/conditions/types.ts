import type { UMB_USER_ALLOW_CHANGE_PASSWORD_CONDITION_ALIAS } from './allow-change-password/constants.js';
import type { UMB_USER_ALLOW_DELETE_CONDITION_ALIAS } from './allow-delete/constants.js';
import type { UMB_USER_ALLOW_DISABLE_CONDITION_ALIAS } from './allow-disable/constants.js';
import type { UMB_USER_ALLOW_ENABLE_CONDITION_ALIAS } from './allow-enable/constants.js';
import type { UMB_USER_ALLOW_EXTERNAL_LOGIN_CONDITION_ALIAS } from './allow-external-login/constants.js';
import type { UMB_USER_ALLOW_MFA_CONDITION_ALIAS } from './allow-mfa/constants.js';
import type { UMB_USER_ALLOW_UNLOCK_CONDITION_ALIAS } from './allow-unlock/constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

type UmbUserConditionConfigs =
	| UmbConditionConfigBase<typeof UMB_USER_ALLOW_CHANGE_PASSWORD_CONDITION_ALIAS>
	| UmbConditionConfigBase<typeof UMB_USER_ALLOW_DELETE_CONDITION_ALIAS>
	| UmbConditionConfigBase<typeof UMB_USER_ALLOW_DISABLE_CONDITION_ALIAS>
	| UmbConditionConfigBase<typeof UMB_USER_ALLOW_ENABLE_CONDITION_ALIAS>
	| UmbConditionConfigBase<typeof UMB_USER_ALLOW_EXTERNAL_LOGIN_CONDITION_ALIAS>
	| UmbConditionConfigBase<typeof UMB_USER_ALLOW_MFA_CONDITION_ALIAS>
	| UmbConditionConfigBase<typeof UMB_USER_ALLOW_UNLOCK_CONDITION_ALIAS>;

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbUserConditionConfigs: UmbUserConditionConfigs;
	}
}
