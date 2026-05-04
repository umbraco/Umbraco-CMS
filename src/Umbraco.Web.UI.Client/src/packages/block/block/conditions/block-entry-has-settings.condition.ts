import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { BlockEntryHasSettingsConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryHasSettingsCondition
	extends UmbConditionBase<BlockEntryHasSettingsConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<BlockEntryHasSettingsConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.settingsElementTypeKey,
				(settingsElementTypeKey) => {
					this.permitted = !!settingsElementTypeKey;
				},
				'observeSettingsElementTypeKey',
			);
		});
	}
}

export default UmbBlockEntryHasSettingsCondition;
