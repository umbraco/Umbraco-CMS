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

		this.consumeContext(UMB_ROUTE_CONTEXT, (context) => {
			// Route context found, so we ARE in a routable context, if that matches the expected match value, then we are permitted.
			this.permitted = !!context === matchValue;
		})
			.asPromise()
			.catch(() => {
				// The context could not be found, so it is absent: permitted if that matches the expected match value.
				this.permitted = !matchValue;
			});
	}
}
