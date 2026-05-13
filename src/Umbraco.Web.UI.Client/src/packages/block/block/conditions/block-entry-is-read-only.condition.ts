import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { BlockEntryIsReadOnlyConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryIsReadOnlyCondition
	extends UmbConditionBase<BlockEntryIsReadOnlyConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<BlockEntryIsReadOnlyConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.readOnlyGuard.permitted,
				(isReadOnly) => {
					if (isReadOnly !== undefined) {
						this.permitted = isReadOnly === (this.config.match !== undefined ? this.config.match : true);
					}
				},
				'observeIsReadOnly',
			);
		});
	}
}

export default UmbBlockEntryIsReadOnlyCondition;
