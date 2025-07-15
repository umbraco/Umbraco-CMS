import type { UMB_CURRENT_USER_IS_ADMIN_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbIsAdminConditionConfig
	extends UmbConditionConfigBase<typeof UMB_CURRENT_USER_IS_ADMIN_CONDITION_ALIAS> {}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbIsAdminConditionConfig: UmbIsAdminConditionConfig;
	}
}
