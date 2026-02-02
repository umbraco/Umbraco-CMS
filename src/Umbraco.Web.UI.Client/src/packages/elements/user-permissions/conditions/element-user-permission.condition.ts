import type { UmbElementUserPermissionConditionConfig } from './types.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_ANCESTORS_ENTITY_CONTEXT, UMB_ENTITY_CONTEXT, type UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ElementPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbElementUserPermissionCondition
	extends UmbConditionBase<UmbElementUserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	#entityType: string | undefined;
	#unique: string | null | undefined;
	#elementPermissions: Array<ElementPermissionPresentationModel> = [];
	#fallbackPermissions: string[] = [];
	#ancestors: Array<UmbEntityUnique> = [];

	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbElementUserPermissionConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => {
					this.#elementPermissions = currentUser?.permissions?.filter(this.#isElementUserPermission) || [];
					this.#fallbackPermissions = currentUser?.fallbackPermissions || [];
					this.#checkPermissions();
				},
				'umbUserPermissionConditionObserver',
			);
		});

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			if (!context) {
				this.removeUmbControllerByAlias('umbUserPermissionEntityContextObserver');
				return;
			}

			this.observe(
				observeMultiple([context.entityType, context.unique]),
				([entityType, unique]) => {
					this.#entityType = entityType;
					this.#unique = unique;
					this.#checkPermissions();
				},
				'umbUserPermissionEntityContextObserver',
			);
		});

		this.consumeContext(UMB_ANCESTORS_ENTITY_CONTEXT, (instance) => {
			this.observe(
				instance?.ancestors,
				(ancestors) => {
					this.#ancestors = ancestors?.map((item) => item.unique) ?? [];
					this.#checkPermissions();
				},
				'observeAncestors',
			);
		});
	}

	#checkPermissions() {
		if (!this.#entityType) return;
		if (this.#unique === undefined) return;

		const hasElementPermissions = this.#elementPermissions.length > 0;

		// if there is no permissions for any elements we use the fallback permissions
		if (!hasElementPermissions) {
			this.#check(this.#fallbackPermissions);
			return;
		}

		// If there are element permissions, we need to check the full path to see if any permissions are defined for the current element
		// If we find multiple permissions in the same path, we will apply the closest one
		if (hasElementPermissions) {
			// Path including the current element and all ancestors
			const path = [...this.#ancestors, this.#unique].filter((unique) => unique !== null);
			// Reverse the path to find the closest element permission quickly
			const reversedPath = [...path].reverse();
			const elementPermissionsMap = new Map(this.#elementPermissions.map((p) => [p.element.id, p]));

			// Find the closest element permission in the path
			const closestElementPermission = reversedPath.find((id) => elementPermissionsMap.has(id));

			// Retrieve the corresponding permission data
			const match = closestElementPermission ? elementPermissionsMap.get(closestElementPermission) : undefined;

			// no permissions for the current element - use the fallback permissions
			if (!match) {
				this.#check(this.#fallbackPermissions);
				return;
			}

			// we found permissions - check them
			this.#check(match.verbs);
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

		this.permitted = allOfPermitted && oneOfPermitted;
	}

	#isElementUserPermission(permission: unknown): permission is ElementPermissionPresentationModel {
		return (permission as ElementPermissionPresentationModel).$type === 'ElementPermissionPresentationModel';
	}
}

export { UmbElementUserPermissionCondition as api };
