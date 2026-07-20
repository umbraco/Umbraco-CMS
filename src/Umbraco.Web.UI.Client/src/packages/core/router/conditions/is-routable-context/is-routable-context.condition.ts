import { UMB_ROUTE_CONTEXT } from '../../route/route.context.js';
import type { UmbIsRoutableContextConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbIsRoutableContextCondition
	extends UmbConditionBase<UmbIsRoutableContextConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbIsRoutableContextConditionConfig>) {
		super(host, args);

		// Default match value is true (checking if we are in a routable context)
		const matchValue = this.config.match ?? true;

		// Default state: we assume we are NOT in a routable context.
		// So if match is false (looking for "not in routable context"), we start as permitted.
		// If match is true (looking for "in routable context"), we start as not permitted.
		this.permitted = !matchValue;

		this.consumeContext(UMB_ROUTE_CONTEXT, () => {
			// Route context found, so we ARE in a routable context.
			// Permitted if match is true, not permitted if match is false.
			this.permitted = matchValue;
		});
	}
}
