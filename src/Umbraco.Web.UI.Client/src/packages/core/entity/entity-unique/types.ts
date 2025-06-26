import type { UMB_ENTITY_UNIQUE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbEntityUniqueConditionConfig = UmbConditionConfigBase<typeof UMB_ENTITY_UNIQUE_CONDITION_ALIAS> & {
	match: string;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbEntityUniqueConditionConfig: UmbEntityUniqueConditionConfig;
	}
}
