import type { UmbLanguageUserPermissionConditionConfig } from './types.js';
import { UMB_CURRENT_USER_CONTEXT, type UmbCurrentUserModel } from '@umbraco-cms/backoffice/current-user';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbLanguageUserPermissionCondition
	extends UmbConditionBase<UmbLanguageUserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbLanguageUserPermissionConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => {
					this.#check(currentUser);
				},
				'umbLanguageUserPermissionConditionObserver',
			);
		});
	}

	#check(currentUser?: UmbCurrentUserModel) {
		if (currentUser?.hasAccessToAllLanguages) {
			this.permitted = true;
			return;
		}
		const cultures = currentUser?.languages ?? [];
		/* we default to true se we don't require both allOf and oneOf to be defined
		 but they can be combined for more complex scenarios */
		let allOfPermitted = true;
		let oneOfPermitted = true;

		// check if all of the verbs are present
		if (this.config.allOf?.length) {
			allOfPermitted = this.config.allOf.every((verb) => cultures.includes(verb));
		}

		// check if at least one of the verbs is present
		if (this.config.oneOf?.length) {
			oneOfPermitted = this.config.oneOf.some((verb) => cultures.includes(verb));
		}

		this.permitted = allOfPermitted && oneOfPermitted;
	}
}

export { UmbLanguageUserPermissionCondition as api };
