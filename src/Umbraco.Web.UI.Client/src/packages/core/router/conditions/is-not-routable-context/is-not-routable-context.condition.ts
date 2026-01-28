import { UMB_ROUTE_CONTEXT } from '../../route/route.context.js';
import type { UmbIsNotRoutableContextConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbIsNotRoutableContextCondition
	extends UmbConditionBase<UmbIsNotRoutableContextConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbIsNotRoutableContextConditionConfig>) {
		super(host, args);
		this.permitted = true;
		this.consumeContext(UMB_ROUTE_CONTEXT, (context) => {
			this.permitted = !context;
		});
	}
}
