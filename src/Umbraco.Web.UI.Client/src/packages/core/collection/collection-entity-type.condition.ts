import { UMB_COLLECTION_CONTEXT } from './collection.context.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbCollectionEntityTypeCondition extends UmbBaseController implements UmbExtensionCondition {
	config: CollectionEntityTypeConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<CollectionEntityTypeConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.permitted = context.getEntityType().toLowerCase() === this.config.match.toLowerCase();
			this.#onChange();
		});
	}
}

export type CollectionEntityTypeConditionConfig = UmbConditionConfigBase<'Umb.Condition.CollectionEntityType'> & {
	/**
	 * Define the workspace that this extension should be available in
	 *
	 * @example
	 * "Document"
	 */
	match: string;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Collection Entity Type Condition',
	alias: 'Umb.Condition.CollectionEntityType',
	api: UmbCollectionEntityTypeCondition,
};
