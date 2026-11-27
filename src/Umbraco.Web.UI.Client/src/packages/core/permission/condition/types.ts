import type { UmbPermissionConfig } from '../types.js';
import type { UMB_PERMISSION_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbPermissionConditionConfig
	extends UmbConditionConfigBase<typeof UMB_PERMISSION_CONDITION_ALIAS>, UmbPermissionConfig {
	/**
	 * Every verb must be permitted.
	 * @example ["Umb.Document.Update"]
	 */
	allOf?: Array<string>;
	/**
	 * At least one verb must be permitted.
	 * @example ["Umb.Document.Publish", "Umb.Document.Unpublish"]
	 */
	oneOf?: Array<string>;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbPermissionConditionConfig: UmbPermissionConditionConfig;
	}
}
