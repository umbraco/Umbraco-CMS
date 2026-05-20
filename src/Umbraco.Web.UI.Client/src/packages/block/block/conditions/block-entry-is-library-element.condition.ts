import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { BlockEntryIsLibraryElementConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryIsLibraryElementCondition
	extends UmbConditionBase<BlockEntryIsLibraryElementConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<BlockEntryIsLibraryElementConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.isLibraryElement,
				(isLibrary) => {
					this.permitted = isLibrary === (this.config.match !== undefined ? this.config.match : true);
				},
				'observeIsLibraryElement',
			);
		});
	}
}

export default UmbBlockEntryIsLibraryElementCondition;
