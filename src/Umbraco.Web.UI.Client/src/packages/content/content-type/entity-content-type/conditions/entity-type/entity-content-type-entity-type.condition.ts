import { UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT } from '../../context/entity-content-type.context-token.js';
import type { UmbEntityContentTypeEntityTypeConditionConfig } from './types.js';
import { UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS } from './constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

const ObserveSymbol = Symbol();

export class UmbEntityContentTypeEntityTypeCondition
	extends UmbConditionBase<UmbEntityContentTypeEntityTypeConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbEntityContentTypeEntityTypeConditionConfig>,
	) {
		super(host, args);

		let permissionCheck: ((entityType: string | undefined) => boolean) | undefined = undefined;
		if (this.config.match) {
			permissionCheck = (entityType: string | undefined) => entityType === this.config.match;
		} else if (this.config.oneOf) {
			permissionCheck = (entityType: string | undefined) =>
				entityType ? this.config.oneOf!.includes(entityType) : false;
		}

		if (permissionCheck !== undefined) {
			this.consumeContext(UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT, (context) => {
				this.observe(
					context?.entityType,
					(entityType) => {
						this.permitted = permissionCheck!(entityType);
					},
					ObserveSymbol,
				);
			});
		} else {
			throw new Error(
				`Condition [${UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS}] could not be initialized properly. Either "match" or "oneOf" must be defined.`,
			);
		}
	}
}
