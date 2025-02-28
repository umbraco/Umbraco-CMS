import type { UmbContextualUserPermissionModel } from '../types.js';
import type { UmbContextualUserPermissionConditionConfig } from './types.js';
import { isContextualUserPermission } from './utils.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

// Do not export - for internal use only
type UmbOnChangeCallbackType = (permitted: boolean) => void;

export class UmbContextualUserPermissionCondition extends UmbControllerBase implements UmbExtensionCondition {
	config: UmbContextualUserPermissionConditionConfig;
	permitted = false;

	#contextualPermissions: Array<UmbContextualUserPermissionModel> = [];
	#onChange: UmbOnChangeCallbackType;

	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbContextualUserPermissionConditionConfig, UmbOnChangeCallbackType>,
	) {
		super(host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context.currentUser,
				(currentUser) => {
					this.#contextualPermissions = currentUser?.permissions?.filter(isContextualUserPermission) || [];
					this.#checkPermissions();
				},
				'umbUserPermissionConditionObserver',
			);
		});
	}

	#checkPermissions() {
		const hasContextualPermissions = this.#contextualPermissions.length > 0;

		if (!hasContextualPermissions) {
			return;
		}

		const permissionsForCurrentContext = this.#contextualPermissions.find(
			(permission) => permission.context === this.config.context,
		);

		if (!permissionsForCurrentContext) {
			return;
		}

		// we found a permission for the permission context - check the verbs
		this.#check(permissionsForCurrentContext.verbs);
	}

	#check(verbs: Array<string>) {
		/* we default to true se we don't require both allOf and oneOf to be defined
		 but they can be combined for more complex scenarios */
		let allOfPermitted = true;
		let oneOfPermitted = true;

		// check if all of the verbs are present
		if (this.config.allOf?.length) {
			allOfPermitted = this.config.allOf.every((verb) => verbs.includes(verb));
		}

		// check if at least one of the verbs is present
		if (this.config.oneOf?.length) {
			oneOfPermitted = this.config.oneOf.some((verb) => verbs.includes(verb));
		}

		// if neither allOf or oneOf is defined we default to false
		if (!allOfPermitted && !oneOfPermitted) {
			allOfPermitted = false;
			oneOfPermitted = false;
		}

		const permitted = allOfPermitted && oneOfPermitted;
		if (permitted === this.permitted) return;

		this.permitted = permitted;
		this.#onChange(permitted);
	}
}

export { UmbContextualUserPermissionCondition as api };
