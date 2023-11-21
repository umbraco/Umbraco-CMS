import { UMB_COLLECTION_CONTEXT } from './collection-default.context.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbCollectionAliasCondition extends UmbBaseController implements UmbExtensionCondition {
	config: CollectionAliasConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<CollectionAliasConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.permitted = context.getManifest()?.alias === this.config.match;
			this.#onChange();
		});
	}
}

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
	api: UmbCollectionAliasCondition,
};
