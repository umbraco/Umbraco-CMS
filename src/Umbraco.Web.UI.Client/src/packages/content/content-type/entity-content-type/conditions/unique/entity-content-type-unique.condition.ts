import { UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT } from '../../context/entity-content-type.context-token.js';
import type { UmbEntityContentTypeUniqueConditionConfig } from './types.js';
import { UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS } from './constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

const ObserveSymbol = Symbol();

export class UmbEntityContentTypeUniqueCondition
	extends UmbConditionBase<UmbEntityContentTypeUniqueConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbEntityContentTypeUniqueConditionConfig>,
	) {
		super(host, args);

		let permissionCheck: ((unique: string | undefined) => boolean) | undefined = undefined;
		if (this.config.match) {
			permissionCheck = (unique: string | undefined) => unique === this.config.match;
		} else if (this.config.oneOf) {
			permissionCheck = (unique: string | undefined) => (unique ? this.config.oneOf!.includes(unique) : false);
		}

		if (permissionCheck !== undefined) {
			this.consumeContext(UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT, (context) => {
				this.observe(
					context?.unique,
					(unique) => {
						this.permitted = permissionCheck!(unique);
					},
					ObserveSymbol,
				);
			});
		} else {
			throw new Error(
				`Condition [${UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS}] could not be initialized properly. Either "match" or "oneOf" must be defined.`,
			);
		}
	}
}
