import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../index.js';
import type { UmbBlockWorkspaceVariesByVariantConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
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
				observeMultiple([context.content.structure.variesByCulture, context.content.structure.variesBySegment]),
				([variesByCulture, variesBySegment]) => {
					if (variesByCulture === undefined || variesBySegment === undefined) return;
					const varies = variesByCulture || variesBySegment;
					this.permitted = varies === (this.config.match !== undefined ? this.config.match : true);
				},
				'observeVariesByVariant',
			);
		});
	}
}

export default UmbBlockWorkspaceVariesByVariantCondition;
