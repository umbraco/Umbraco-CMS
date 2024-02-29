import { UMB_DEFAULT_COLLECTION_CONTEXT } from './default/collection-default.context.js';
import type { CollectionAliasConditionConfig } from './collection-alias.manifest.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbCollectionAliasCondition extends UmbBaseController implements UmbExtensionCondition {
	config: CollectionAliasConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<CollectionAliasConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (context) => {
			this.permitted = context.getManifest()?.alias === this.config.match;
			this.#onChange();
		});
	}
}

export default UmbCollectionAliasCondition;
