import type { UMB_COLLECTION_ALIAS_CONDITION } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/naming-convention
export type CollectionAliasConditionConfig = UmbConditionConfigBase<typeof UMB_COLLECTION_ALIAS_CONDITION> & {
	/**
	 * The collection that this extension should be available in
	 * @example
	 * "Umb.Collection.User"
	 */
	match: string;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		CollectionAliasConditionConfig: CollectionAliasConditionConfig;
	}
}
