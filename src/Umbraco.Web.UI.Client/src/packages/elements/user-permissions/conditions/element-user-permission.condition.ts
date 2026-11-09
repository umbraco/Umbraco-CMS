import type { UmbElementUserPermissionConditionConfig } from './types.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_ANCESTORS_ENTITY_CONTEXT, UMB_ENTITY_CONTEXT, type UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	IPermissionPresentationModelElementContainerPermissionPresentationModel,
	IPermissionPresentationModelElementPermissionPresentationModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

type ElementOrContainerPermission =
	| IPermissionPresentationModelElementPermissionPresentationModel
	| IPermissionPresentationModelElementContainerPermissionPresentationModel;

export class UmbElementUserPermissionCondition
	extends UmbConditionBase<UmbElementUserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	#entityType: string | undefined;
	#unique: string | null | undefined;
	#elementPermissions?: Array<ElementOrContainerPermission>;
	#fallbackPermissions?: Array<string>;
	#ancestors: Array<UmbEntityUnique> = [];

	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbElementUserPermissionConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => {
					this.#elementPermissions = currentUser?.permissions?.filter(this.#isElementUserPermission);
					this.#fallbackPermissions = currentUser?.fallbackPermissions;
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

		// Wait until the current user's permissions have loaded before evaluating,
		// so we don't permit/deny based on incomplete data.
		if (this.#elementPermissions === undefined) return;
		if (this.#fallbackPermissions === undefined) return;

		// If no permission is defined on the path to the current element we use the fallback permissions
		this.#check(this.#findClosestVerbs(this.#elementPermissions) ?? this.#fallbackPermissions);
	}

	#findClosestVerbs(permissions: Array<ElementOrContainerPermission>): Array<string> | undefined {
		// Element entries are inserted last so they take precedence over a container entry with the same id
		const permissionsById = new Map<string, ElementOrContainerPermission>();
		for (const p of permissions) {
			if (p.$type === 'ElementContainerPermissionPresentationModel') {
				permissionsById.set(p.elementContainer.id, p);
			}
		}
		for (const p of permissions) {
			if (p.$type === 'ElementPermissionPresentationModel') {
				permissionsById.set(p.element.id, p);
			}
		}

		// Walk from the current element up through its ancestors to find the closest permission
		const path = [...this.#ancestors, this.#unique].filter((unique): unique is string => typeof unique === 'string');
		const closestId = path.reverse().find((id) => permissionsById.has(id));
		return closestId ? permissionsById.get(closestId)?.verbs : undefined;
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

	#isElementUserPermission(permission: unknown): permission is ElementOrContainerPermission {
		const type = (permission as ElementOrContainerPermission).$type;
		return type === 'ElementPermissionPresentationModel' || type === 'ElementContainerPermissionPresentationModel';
	}
}

export { UmbElementUserPermissionCondition as api };
