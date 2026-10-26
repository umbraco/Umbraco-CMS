import { UmbSchemaOperationAllowedController } from '../schema-operation-allowed.controller.js';
import type { UmbSchemaOperationAllowedConditionConfig } from '../types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSchemaOperationAllowedCondition
	extends UmbConditionBase<UmbSchemaOperationAllowedConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbSchemaOperationAllowedConditionConfig>,
	) {
		super(host, args);

		const operationAllowed = new UmbSchemaOperationAllowedController(this, {
			entityType: this.config.entityType,
			operation: this.config.operation,
		});

		this.observe(
			operationAllowed.allowed,
			(allowed) => {
				this.permitted = allowed === (this.config.match ?? true);
			},
			null,
		);
	}
}
