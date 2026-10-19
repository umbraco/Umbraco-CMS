import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { BlockEntryHasExternalContentConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryHasExternalContentCondition
	extends UmbConditionBase<BlockEntryHasExternalContentConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<BlockEntryHasExternalContentConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.isExternalContent,
				(isExternalContent) => {
					this.permitted = isExternalContent === (this.config.match !== undefined ? this.config.match : true);
				},
				'observeIsExternalContent',
			);
		});
	}
}

export default UmbBlockEntryHasExternalContentCondition;
