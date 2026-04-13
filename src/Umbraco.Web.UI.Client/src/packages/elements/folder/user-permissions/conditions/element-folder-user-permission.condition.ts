import type { UmbElementFolderUserPermissionConditionConfig } from './types.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_ANCESTORS_ENTITY_CONTEXT, UMB_ENTITY_CONTEXT, type UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ElementContainerPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbElementFolderUserPermissionCondition
	extends UmbConditionBase<UmbElementFolderUserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	#entityType: string | undefined;
	#unique: string | null | undefined;
	#elementFolderPermissions: Array<ElementContainerPermissionPresentationModel> = [];
	#fallbackPermissions: string[] = [];
	#ancestors: Array<UmbEntityUnique> = [];

	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbElementFolderUserPermissionConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => {
					this.#elementFolderPermissions = currentUser?.permissions?.filter(this.#isElementFolderUserPermission) || [];
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

		const hasElementFolderPermissions = this.#elementFolderPermissions.length > 0;

		// if there is no permissions for any elements we use the fallback permissions
		if (!hasElementFolderPermissions) {
			this.#check(this.#fallbackPermissions);
			return;
		}

		// If there are element folder permissions, we need to check the full path to see if any permissions are defined for the current element
		// If we find multiple permissions in the same path, we will apply the closest one
		if (hasElementFolderPermissions) {
			// Path including the current element and all ancestors
			const path = [...this.#ancestors, this.#unique].filter((unique) => unique !== null);
			// Reverse the path to find the closest element folder permission quickly
			const reversedPath = [...path].reverse();
			const elementFolderPermissionsMap = new Map(
				this.#elementFolderPermissions.map((p) => [p.elementContainer.id, p]),
			);

			// Find the closest element folder permission in the path
			const closestElementFolderPermission = reversedPath.find((id) => elementFolderPermissionsMap.has(id));

			// Retrieve the corresponding permission data
			const match = closestElementFolderPermission
				? elementFolderPermissionsMap.get(closestElementFolderPermission)
				: undefined;

			// no permissions for the current element folder - use the fallback permissions
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

	#isElementFolderUserPermission(permission: unknown): permission is ElementContainerPermissionPresentationModel {
		return (
			(permission as ElementContainerPermissionPresentationModel).$type ===
			'ElementContainerPermissionPresentationModel'
		);
	}
}

export { UmbElementFolderUserPermissionCondition as api };
