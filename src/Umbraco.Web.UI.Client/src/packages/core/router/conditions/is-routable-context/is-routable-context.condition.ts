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
		this.consumeContext(UMB_ROUTE_CONTEXT, (context) => {
			this.permitted = !!context;
		});
	}
}
