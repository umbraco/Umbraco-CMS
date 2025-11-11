import { isDocumentUserPermission } from '../utils.js';
import type { UmbDocumentUserPermissionConditionConfig } from './types.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_ANCESTORS_ENTITY_CONTEXT, UMB_ENTITY_CONTEXT, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbDocumentUserPermissionCondition
	extends UmbConditionBase<UmbDocumentUserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	#entityType: string | undefined;
	#unique: string | null | undefined;
	#documentPermissions: Array<DocumentPermissionPresentationModel> = [];
	#fallbackPermissions: string[] = [];
	#ancestors: Array<UmbEntityModel> = [];

	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbDocumentUserPermissionConditionConfig>,
	) {
		super(host, args);

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
					this.#ancestors = ancestors ?? [];
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

		// If there are document permissions, we need to check the full path to see if any permissions are defined for the current entity
		// If we find multiple permissions in the same path, we will apply the closest one
		if (hasDocumentPermissions) {
			// Path including the current document and all ancestors, reversed to find the closest permission
			const reversedPath = [
				...this.#ancestors,
				{
					entityType: this.#entityType,
					unique: this.#unique,
				},
			]
				.filter((m) => m.entityType === UMB_DOCUMENT_ENTITY_TYPE && m.unique !== null)
				.reverse();

			const documentPermissionsMap: Map<string | null, DocumentPermissionPresentationModel> = new Map(
				this.#documentPermissions.map((m) => [m.document.id, m]),
			);

			// Find the closest document with permissions in the path
			const closestDocumentWithPermission = reversedPath.find((m) => documentPermissionsMap.has(m.unique));

			// Retrieve the corresponding permission data
			const match = closestDocumentWithPermission
				? documentPermissionsMap.get(closestDocumentWithPermission.unique)
				: undefined;

			if (match) {
				// we found permissions - check them
				this.#check(match.verbs);
				return;
			}
		}

		// if there is no permissions for any documents we use the fallback permissions
		this.#check(this.#fallbackPermissions);
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
}

export { UmbDocumentUserPermissionCondition as api };
