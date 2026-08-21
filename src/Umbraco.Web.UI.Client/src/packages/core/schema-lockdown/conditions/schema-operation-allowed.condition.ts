import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from '../schema-lockdown.context-token.js';
import type { UmbSchemaOperationAllowedConditionConfig } from '../types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const ObserveSymbol = Symbol();

export class UmbSchemaOperationAllowedCondition
	extends UmbConditionBase<UmbSchemaOperationAllowedConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbSchemaOperationAllowedConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_SCHEMA_LOCKDOWN_CONTEXT, (context) => {
			this.observe(
				context?.state,
				() => {
					const allowed = context?.isAllowed(this.config.entityType, this.config.operation);
					if (allowed !== undefined) {
						this.permitted = allowed === (this.config.match ?? true);
					}
				},
				ObserveSymbol,
			);
		});
	}
}
