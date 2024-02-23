import type { ManifestCondition, UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type CollectionAliasConditionConfig = UmbConditionConfigBase<typeof UMB_COLLECTION_ALIAS_CONDITION> & {
	/**
	 * The collection that this extension should be available in
	 *
	 * @example
	 * "Umb.Collection.User"
	 */
	match: string;
};

export const UMB_COLLECTION_ALIAS_CONDITION = 'Umb.Condition.CollectionAlias';
export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Collection Alias Condition',
	alias: UMB_COLLECTION_ALIAS_CONDITION,
	api: () => import('./collection-alias.condition.js'),
};
