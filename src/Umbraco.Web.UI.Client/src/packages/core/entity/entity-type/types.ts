import type { UMB_ENTITY_TYPE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbEntityTypeConditionConfig = UmbConditionConfigBase<typeof UMB_ENTITY_TYPE_CONDITION_ALIAS> & {
	allOf?: Array<string>;
	match: string;
	oneOf?: Array<string>;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbEntityTypeConditionConfig: UmbEntityTypeConditionConfig;
	}
}
