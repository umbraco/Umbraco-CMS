import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../index.js';
import type { UmbBlockWorkspaceVariesByVariantConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockWorkspaceVariesByVariantCondition
	extends UmbConditionBase<UmbBlockWorkspaceVariesByVariantConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbBlockWorkspaceVariesByVariantConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.variesByVariant,
				(varies) => {
					if (varies !== undefined) {
						this.permitted = varies === (this.config.match ?? true);
					}
				},
				'observeVariesByVariant',
			);
		});
	}
}

export default UmbBlockWorkspaceVariesByVariantCondition;
