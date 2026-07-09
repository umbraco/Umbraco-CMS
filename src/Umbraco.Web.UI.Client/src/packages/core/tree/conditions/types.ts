import type { UMB_TREE_ALIAS_CONDITION } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/naming-convention
export type TreeAliasConditionConfig = UmbConditionConfigBase<typeof UMB_TREE_ALIAS_CONDITION> & {
	/**
	 * The tree that this extension should be available in.
	 * @example
	 * "Umb.Tree.DocumentType"
	 */
	match: string;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		TreeAliasConditionConfig: TreeAliasConditionConfig;
	}
}
