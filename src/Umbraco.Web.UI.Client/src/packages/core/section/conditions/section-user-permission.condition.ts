import type { UmbSectionUserPermissionConditionConfig } from './types.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

const ObserveSymbol = Symbol();

export class UmbSectionUserPermissionCondition
	extends UmbConditionBase<UmbSectionUserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbSectionUserPermissionConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => {
					const allowedSections = currentUser?.allowedSections ?? [];
					this.permitted = allowedSections.includes(this.config.match);
				},
				ObserveSymbol,
			);
		});
	}
}
