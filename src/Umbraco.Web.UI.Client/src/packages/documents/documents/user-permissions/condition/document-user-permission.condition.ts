import { UMB_CURRENT_USER_CONTEXT } from '../../../../user/current-user/current-user.context.js';
import { isDocumentUserPermission } from '../utils.js';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentUserPermissionCondition
	extends UmbConditionBase<UmbDocumentUserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	#entityType: string | undefined;
	#unique: string | null | undefined;
	#documentPermissions: Array<DocumentPermissionPresentationModel> = [];
	#fallbackPermissions: string[] = [];

	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbDocumentUserPermissionConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context.currentUser,
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

		/* If there are document permission we check if there are permissions for the current document
		 If there aren't we use the fallback permissions */
		if (hasDocumentPermissions) {
			const permissionsForCurrentDocument = this.#documentPermissions.find(
				(permission) => permission.document.id === this.#unique,
			);

			// no permissions for the current document - use the fallback permissions
			if (!permissionsForCurrentDocument) {
				this.#check(this.#fallbackPermissions);
				return;
			}

			// we found permissions for the current document - check them
			this.#check(permissionsForCurrentDocument.verbs);
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
}

export type UmbDocumentUserPermissionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.UserPermission.Document'> & {
		/**
		 * The user must have all of the permissions in this array for the condition to be met.
		 *
		 * @example
		 * ["Umb.Document.Save", "Umb.Document.Publish"]
		 */
		allOf?: Array<string>;

		/**
		 * The user must have at least one of the permissions in this array for the condition to be met.
		 *
		 * @example
		 * ["Umb.Document.Save", "Umb.Document.Publish"]
		 */
		oneOf?: Array<string>;
	};

export { UmbDocumentUserPermissionCondition as api };
