import { UMB_SEARCH_WORKSPACE_CONTEXT } from '../workspace/search-workspace.context-token.js';
import type { UmbSearchIndexProviderNameConditionConfig } from './types.js';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';

export class UmbSearchIndexProviderNameCondition
	extends UmbConditionBase<UmbSearchIndexProviderNameConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbSearchIndexProviderNameConditionConfig>,
	) {
		super(host, args);

		const matchArray = args.config.oneOf ?? (args.config.match ? [args.config.match] : undefined) ?? [];

		this.consumeContext(UMB_SEARCH_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.providerName,
				(providerName) => {
					this.permitted = providerName ? stringOrStringArrayContains(matchArray, providerName) : false;
				},
				'_observeProviderName',
			);
		});
	}
}

export default UmbSearchIndexProviderNameCondition;
