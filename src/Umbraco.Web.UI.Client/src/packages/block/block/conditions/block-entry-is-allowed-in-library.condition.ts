import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import type { BlockEntryIsAllowedInLibraryConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockEntryIsAllowedInLibraryCondition
	extends UmbConditionBase<BlockEntryIsAllowedInLibraryConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<BlockEntryIsAllowedInLibraryConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.isAllowedInLibrary,
				(isAllowedInLibrary) => {
					if (isAllowedInLibrary !== undefined) {
						this.permitted = isAllowedInLibrary === (this.config.match !== undefined ? this.config.match : true);
					}
				},
				'observeIsAllowedInLibrary',
			);
		});
	}
}

export default UmbBlockEntryIsAllowedInLibraryCondition;
