import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { BlockEntryHasSharedContentConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryHasSharedContentCondition
	extends UmbConditionBase<BlockEntryHasSharedContentConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<BlockEntryHasSharedContentConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.isSharedContent,
				(isSharedContent) => {
					this.permitted = isSharedContent === (this.config.match !== undefined ? this.config.match : true);
				},
				'observeIsSharedContent',
			);
		});
	}
}

export default UmbBlockEntryHasSharedContentCondition;
