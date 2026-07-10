import type { UMB_COLLECTION_CONDITION_ALIAS, UMB_COLLECTION_HAS_ITEMS_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/naming-convention
export type CollectionAliasConditionConfig = UmbConditionConfigBase<typeof UMB_COLLECTION_CONDITION_ALIAS> & {
	/**
	 * The collection that this extension should be available in
	 * @example
	 * "Umb.Collection.User"
	 */
	match: string;
};

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbCollectionHasItemsConditionConfig
	extends UmbConditionConfigBase<typeof UMB_COLLECTION_HAS_ITEMS_CONDITION_ALIAS> {}

declare global {
	interface UmbExtensionConditionConfigMap {
		CollectionAliasConditionConfig: CollectionAliasConditionConfig;
		UmbCollectionHasItemsConditionConfig: UmbCollectionHasItemsConditionConfig;
	}
}
