import type { UmbIsProductionModeConditionConfig } from './types.js';
import { UMB_SERVER_CONTEXT } from '../server.context-token.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const ObserveSymbol = Symbol();

export class UmbIsProductionModeCondition
	extends UmbConditionBase<UmbIsProductionModeConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbIsProductionModeConditionConfig>) {
		super(host, args);

		// Default to not permitted until we know the server's runtime mode (safe default).
		this.permitted = false;

		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this.observe(
				context?.isProductionMode,
				(isProduction) => {
					if (isProduction !== undefined) {
						this.permitted = isProduction === (this.config.match ?? true);
					}
				},
				ObserveSymbol,
			);
		});
	}
}
