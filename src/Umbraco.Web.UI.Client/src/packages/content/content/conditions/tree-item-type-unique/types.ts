import type { UMB_CONTENT_TREE_ITEM_TYPE_UNIQUE_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbContentTreeItemTypeUniqueConditionConfig = UmbConditionConfigBase<
	typeof UMB_CONTENT_TREE_ITEM_TYPE_UNIQUE_CONDITION_ALIAS
> & {
	match?: string;
	oneOf?: Array<string>;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbContentTreeItemTypeUniqueConditionConfig: UmbContentTreeItemTypeUniqueConditionConfig;
	}
}
