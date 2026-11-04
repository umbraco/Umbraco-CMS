import { UmbSchemaOperationAllowedController } from '../schema-operation-allowed.controller.js';
import type { UmbSchemaOperationAllowedConditionConfig } from '../types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

/**
 * Permits an extension while schema lockdown allows the configured operation on the entity type, or, with
 * `match: false`, while it denies it. The entity type defaults to the one of the surrounding workspace.
 */
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

		if (!this.config.entityType) {
			this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) =>
				operationAllowed.setEntityType(context?.getEntityType()),
			);
		}

		this.observe(
			operationAllowed.allowed,
			(allowed) => {
				this.permitted = allowed === (this.config.match ?? true);
			},
			null,
		);
	}
}
