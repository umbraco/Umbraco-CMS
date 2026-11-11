import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { UmbBlockEntryVariesByVariantConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryVariesByVariantCondition
	extends UmbConditionBase<UmbBlockEntryVariesByVariantConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbBlockEntryVariesByVariantConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.variesByVariant,
				(varies) => {
					if (varies !== undefined) {
						this.permitted = varies === (this.config.match !== undefined ? this.config.match : true);
					}
				},
				'observeVariesByVariant',
			);
		});
	}
}

export default UmbBlockEntryVariesByVariantCondition;
