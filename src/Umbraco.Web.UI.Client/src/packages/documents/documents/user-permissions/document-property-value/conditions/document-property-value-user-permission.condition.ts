import type { UmbDocumentPropertyValueUserPermissionModel } from '../types.js';
import type { UmbDocumentPropertyValueUserPermissionConditionConfig } from './types.js';
import { isDocumentPropertyValueUserPermission } from './utils.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

// Do not export - for internal use only
type UmbOnChangeCallbackType = (permitted: boolean) => void;

export class UmbDocumentPropertyValueUserPermissionCondition
	extends UmbControllerBase
	implements UmbExtensionCondition
{
	config: UmbDocumentPropertyValueUserPermissionConditionConfig;
	permitted = false;

	#documentPropertyValuePermissions: Array<UmbDocumentPropertyValueUserPermissionModel> = [];
	#fallbackPermissions: string[] = [];
	#onChange: UmbOnChangeCallbackType;

	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<
			UmbDocumentPropertyValueUserPermissionConditionConfig,
			UmbOnChangeCallbackType
		>,
	) {
		super(host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context.currentUser,
				(currentUser) => {
					this.#documentPropertyValuePermissions =
						currentUser?.permissions?.filter(isDocumentPropertyValueUserPermission) || [];
					this.#fallbackPermissions = currentUser?.fallbackPermissions || [];
					this.#checkPermissions();
				},
				'umbDocumentPropertyValueUserPermissionConditionObserver',
			);
		});
	}

	#checkPermissions() {
		const hasDocumentPropertyValuePermissions = this.#documentPropertyValuePermissions.length > 0;

		// if there is no permissions for any documents we use the fallback permissions
		if (!hasDocumentPropertyValuePermissions) {
			this.#check(this.#fallbackPermissions);
			return;
		}

		/* If there are document permission we check if there are permissions for the current document property value
		 If there aren't we use the fallback permissions */
		if (hasDocumentPropertyValuePermissions) {
			const permissionsForCurrentDocumentPropertyValue = this.#documentPropertyValuePermissions.find(
				(permission) => permission.propertyType.unique === this.config.match.propertyType.unique,
			);

			// no permissions for the current document property value - use the fallback permissions
			if (!permissionsForCurrentDocumentPropertyValue) {
				this.#check(this.#fallbackPermissions);
				return;
			}

			// we found permissions for the current document property value - check them
			this.#check(permissionsForCurrentDocumentPropertyValue.verbs);
		}
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

export { UmbDocumentPropertyValueUserPermissionCondition as api };
