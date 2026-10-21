import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { BlockEntryIsUnsupportedConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryIsUnsupportedCondition
	extends UmbConditionBase<BlockEntryIsUnsupportedConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<BlockEntryIsUnsupportedConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.observe(
				context?.unsupported,
				(unsupported) => {
					if (unsupported !== undefined) {
						this.permitted = unsupported === (this.config.match !== undefined ? this.config.match : true);
					}
				},
				'observeIsUnsupported',
			);
		});
	}
}

export default UmbBlockEntryIsUnsupportedCondition;
