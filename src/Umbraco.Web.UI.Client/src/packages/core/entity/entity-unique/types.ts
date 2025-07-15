import type { UMB_ENTITY_UNIQUE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbEntityUnique = string | null;

export type UmbEntityUniqueConditionConfig = UmbConditionConfigBase<typeof UMB_ENTITY_UNIQUE_CONDITION_ALIAS> & {
	match?: UmbEntityUnique;
	oneOf?: Array<UmbEntityUnique>;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbEntityUniqueConditionConfig: UmbEntityUniqueConditionConfig;
	}
}
