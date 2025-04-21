import { isDocumentUserPermission } from '../utils.js';
import type { UmbDocumentUserPermissionConditionConfig } from './types.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_ANCESTORS_ENTITY_CONTEXT, UMB_ENTITY_CONTEXT, type UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

// Do not export - for internal use only
type UmbOnChangeCallbackType = (permitted: boolean) => void;

export class UmbDocumentUserPermissionCondition extends UmbControllerBase implements UmbExtensionCondition {
	config: UmbDocumentUserPermissionConditionConfig;
	permitted = false;

	#entityType: string | undefined;
	#unique: string | null | undefined;
	#documentPermissions: Array<DocumentPermissionPresentationModel> = [];
	#fallbackPermissions: string[] = [];
	#onChange: UmbOnChangeCallbackType;
	#ancestors: Array<UmbEntityUnique> = [];

	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbDocumentUserPermissionConditionConfig, UmbOnChangeCallbackType>,
	) {
		super(host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => {
					this.#documentPermissions = currentUser?.permissions?.filter(isDocumentUserPermission) || [];
					this.#fallbackPermissions = currentUser?.fallbackPermissions || [];
					this.#checkPermissions();
				},
				'umbUserPermissionConditionObserver',
			);
		});

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			if (!context) return;

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

		const hasDocumentPermissions = this.#documentPermissions.length > 0;

		// if there is no permissions for any documents we use the fallback permissions
		if (!hasDocumentPermissions) {
			this.#check(this.#fallbackPermissions);
			return;
		}

		// If there are document permissions, we need to check the full path to see if any permissions are defined for the current document
		// If we find multiple permissions in the same path, we will apply the closest one
		if (hasDocumentPermissions) {
			// Path including the current document and all ancestors
			const path = [...this.#ancestors, this.#unique].filter((unique) => unique !== null);
			// Reverse the path to find the closest document permission quickly
			const reversedPath = [...path].reverse();
			const documentPermissionsMap = new Map(this.#documentPermissions.map((p) => [p.document.id, p]));

			// Find the closest document permission in the path
			const closestDocumentPermission = reversedPath.find((id) => documentPermissionsMap.has(id));

			// Retrieve the corresponding permission data
			const match = closestDocumentPermission ? documentPermissionsMap.get(closestDocumentPermission) : undefined;

			// no permissions for the current document - use the fallback permissions
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

		const permitted = allOfPermitted && oneOfPermitted;
		if (permitted === this.permitted) return;

		this.permitted = permitted;
		this.#onChange(permitted);
	}
}

export { UmbDocumentUserPermissionCondition as api };
